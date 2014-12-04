namespace MIMEMultipart

module internal Generator = // Exposing generateAttachmentString.
  open Ancillary   // For (/=), nl.
  open AttachmentR // For AttachmentR.
  open System      // For Convert, Random, String.
  open System.Text // For StringBuilder.
  open System.Text.RegularExpressions

  let private generateBoundary (r:Random) =
      let next () = r.Next ()
      let nextR m n = r.Next (m,n)
      let nextS = next >> string
      let (n,x) = 37,69 // 37 is completely arbitrary; 69 is documented: min and max lengths of boundary.
      let m = nextR n x // Length of the boundary we'll generate.
      let rec generate a = if String.length a < m then generate (a + nextS ()) else a
      let b = generate ""
       in if b.Length > x then b.Substring (0, x-1) else b

  let private contentLocation text =
      let serviceHeader = new Regex "DOCTYPE ServiceHeader"
      let receiptAck    = new Regex "DOCTYPE ReceiptAcknowledgment"
      let deliveryHead  = new Regex "DOCTYPE DeliveryHeader"
      let preamble      = new Regex "DOCTYPE Preamble"

      match true with
      | _ when serviceHeader.IsMatch text -> "RN-Service-Header"
      | _ when receiptAck.IsMatch text    -> "RN-Service-Content"
      | _ when deliveryHead.IsMatch text  -> "RN-Delivery-Header"
      | _ when preamble.IsMatch text      -> "RN-Preamble"
      | _                                 -> "UNKNOWN"

  let private generateMessageID (r:Random) = "Apex_" + r.Next().ToString()

  let private hasAttachments (a:AttachmentR) =
      match List.ofSeq a.Attachments with
      | [] -> false
      | _  -> true

  let private generateHeader (a:AttachmentR) boundary additionalHeaders =
      let b = if boundary="" then "" else "; boundary=\"" + boundary + "\""
      let sb = new StringBuilder()

      sb.AppendLine ("Content-Type: " + a.ContentType + b) |> ignore

      match additionalHeaders with
      | [] -> ()
      | hs -> List.map sb.AppendLine hs |> ignore

      match a.OriginalEncoding with
      | "" -> ()
      | s  -> sb.AppendLine ("Content-Transfer-Encoding: " + s) |> ignore

      match contentLocation a.Text with
      | "UNKNOWN" -> ()
      | s -> sb.AppendLine ("Content-Location: " + s) |> ignore

      match a.ContentID with
      | "" -> ()
      | s  -> sb.AppendLine ("Content-ID: " + s) |> ignore

      sb.ToString()

  let private wrap columns text =
      let lines = new StringBuilder()
      let n = String.length text
      let rec f c m = if c<n
                      then lines.AppendLine (text.Substring(c, min m columns)) |> ignore
                           f (c+columns) (m-columns)
                      else lines.ToString()
       in if n<columns
          then text
          else (f 0 n).Trim()

  let rec private generateBody r a =
      let columnLimit = 76
       in match true with
          | _ when a.IsByteArray -> ((wrap columnLimit <| Convert.ToBase64String a.Bytes) + nl, "", [])
          | _ when hasAttachments a ->
              let boundary = generateBoundary r
              let boundaryLine = "--" + boundary + nl
              let innerBody = String.Join(boundaryLine, Seq.map (generateAttachmentString r) a.Attachments)
              let body = boundaryLine + innerBody + "--" + boundary + "--" + nl
              let additionalHeaders = [ "Message-ID: " + generateMessageID r
                                      ; "Mime-Version: 1.0" ]
               in (body, boundary, additionalHeaders)
          | _ -> (a.Text, "", [])

  and generateAttachmentString r a =
      let (body, boundary, additionalHeaders) = generateBody r a
      let head = generateHeader a boundary additionalHeaders
       in head + nl + body + nl

// vim:ft=fs