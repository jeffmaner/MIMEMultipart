namespace MIMEMultipart

module Parser = // Exposing readStreamWithHeaders and readStreamWithoutHeaders.
  open Ancillary   // For (/=), nl.
  open AttachmentR // For AttachmentR.
  open System.IO   // For StringReader, TextReader.
  open System.Text // For StringBuilder.
  open System.Text.RegularExpressions

  let private readLine (tr:TextReader) =
      match tr.ReadLine() with
      | null -> None
      | s    -> Some s

  /// Reads boundary value of content-type field value.
  let private readBoundary line =
      let regex = new Regex("(?i)boundary=(\"(?<boundary>.*?)\"|(?<boundary>[a-zA-Z0-9]*))");

      if regex.IsMatch line
      then Some (regex.Match line).Groups.["boundary"].Value
      else None

  /// Unfolds into a single line header fields that have been folded to subsequent lines.
  /// <details>Per [Wikipedia][1], "Historically, long lines could be folded into multiple lines; continuation lines are indicated by the presence of a space (SP) or horizontal tab (HT) as the first character on the next line." [1]: http://en.wikipedia.org/wiki/List_of_HTTP_header_fields</details>
  /// <remarks>This method mutates tr.</remarks>
  let rec private unfold (tr:TextReader) =
      let (space, tab, eof) = ' ', '\t', -1
      let fieldContinues c = c=space || c=tab
      let isEOF = (=) eof
      let n = tr.Peek()
       in if (not (isEOF n)) && fieldContinues (char n)
          then tr.ReadLine() + unfold tr
          else ""

  /// Processes MIME headers.
  /// <returns>Tuple of new Attachment and its associated boundary.</returns>
  /// <remarks>This method mutates tr.</remarks>
  let private digestHeaders tr =
      let newAttachment = { ContentType=""; ContentID=""; OriginalEncoding=""; IsByteArray=false; Bytes=[||]; Text=""; Attachments=[] }
      let rec f s a b =
          match s with
          | None | Some "" -> (a,b)
          | Some t -> let line = t + unfold tr
                      let ps = line.Split [| ':' |]
                       in if ps.Length /= 2
                          then failwith (sprintf "Malformed header field: %s." line)
                          else let (n,v) = ps.[0], ps.[1].Trim()
                                in match n.ToLower() with
                                   | "content-type" -> f (readLine tr) { a with ContentType=v } (readBoundary v)
                                   | "content-id"   -> f (readLine tr) { a with ContentID=v } b
                                   | "content-transfer-encoding" -> f (readLine tr) { a with OriginalEncoding=v } b
                                   | _ -> f (readLine tr) a b
       in f (readLine tr) newAttachment None

  let private decodeBase64 = System.Convert.FromBase64String
  let private decodeQP text =
    Regex.Replace(text, @"=([0-9a-fA-F]{2})|=\r\n",
                  fun (m:Match) -> if m.Groups.[1].Success
                                   then System.Convert.ToChar(System.Convert.ToInt32(m.Groups.[1].Value, 16)).ToString()
                                   else "");

  let rec readStreamWithoutHeaders (tr:TextReader) boundary =
      let boundary' = match boundary with | None -> "" | Some b -> b
      let boundaryLine = "--" + boundary'
      let terminationLine = boundaryLine + "--"
      let rec skipToBoundary () =
          match readLine tr with
          | None -> ()
          | Some s when s=boundaryLine -> ()
          | _ -> skipToBoundary ()
      let rec buildBody (sb:StringBuilder) line =
          match line with
          | None -> sb.ToString()
          | Some s when s=boundaryLine -> sb.ToString()
          | Some s when s=terminationLine -> sb.ToString()
          | Some s -> sb.AppendLine s |> ignore
                      buildBody sb (readLine tr)
      let assemble a b body =
          match b with
          | Some s -> { a with Text=body; Attachments=readStreamWithoutHeaders (new StringReader (body)) b }
          | None   -> match a.OriginalEncoding.ToLower() with
                      | "base64" -> { a with Bytes=decodeBase64 (body.Replace(nl, "")) }
                      | "quoted-printable" -> { a with Text=decodeQP body }
                      | _ -> { a with Text=body }
      let read tr =
          let (a,b) = digestHeaders tr
          let body = buildBody (new StringBuilder ()) (readLine tr)
           in assemble a b body

      skipToBoundary ()

      [ while tr.Peek() > -1 do
          yield read tr ]

  let readStreamWithHeaders (tr:TextReader) b =
      let line = tr.ReadLine() + unfold tr
       in readStreamWithoutHeaders tr <| readBoundary line

// vim:ft=fs