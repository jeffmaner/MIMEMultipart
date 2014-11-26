namespace MIMEMultipart

open Ancillary   // For (/=).
open AttachmentR // For AttachmentR.
open Generator   // For generateAttachmentString.
open Parser      // For readStreamWithHeaders and readStreamWithoutHeaders.
open System      // For Random.

type Attachment() =
    let mutable attachments = Seq.empty
    let mutable bytes = [||]
    let mutable contentID = ""
    let mutable contentType = ""
    let mutable isByteArray = false
    let mutable originalEncoding = ""
    let mutable text = ""

    static let rec toAttachment r =
        let a = new Attachment()
        a.Attachments <- Seq.map toAttachment r.Attachments
        a.Bytes <- r.Bytes
        a.ContentID <- r.ContentID
        a.ContentType <- r.ContentType
        a.IsByteArray <- r.IsByteArray
        a.OriginalEncoding <- r.OriginalEncoding
        a.Text <- r.Text
        a

    let rec toRecord (a:Attachment) =
        { Attachments=Seq.map toRecord a.Attachments
        ; Bytes=a.Bytes
        ; ContentID=a.ContentID
        ; ContentType=a.ContentType
        ; IsByteArray=a.IsByteArray
        ; OriginalEncoding=a.OriginalEncoding
        ; Text=a.Text }

    static let readStream tr b =
        let f = match b with
                | Some s -> readStreamWithoutHeaders
                | None   -> readStreamWithHeaders
         in seq { for r in f tr b do
                    if r.ContentType /= "" // TODO: I don't understand why this is not filtering...
                    then yield toAttachment r }

    member a.GenerateAttachmentString () =
      generateAttachmentString (new Random())
                               { Attachments=attachments
                               ; Bytes=bytes
                               ; ContentID=contentID
                               ; ContentType=contentType
                               ; IsByteArray=isByteArray
                               ; OriginalEncoding=originalEncoding
                               ; Text=text }
    /// Read stream that includes headers.
    static member ReadStreamWithHeaders textReader = readStream textReader None
    /// Read stream the headers of which have already been consumed.
    static member ReadStreamWithoutHeaders textReader boundary =
      readStream textReader (Some boundary)

    member a.Attachments
      with get() = Seq.map toAttachment <| // TODO: This filter should be unnecessary...
                   Seq.filter (fun a -> a.ContentType /= "") attachments
       and set v = attachments <- Seq.map toRecord v
    member a.Bytes with get() = bytes and set v = bytes <- v
    member a.ContentID with get() = contentID and set v = contentID <- v
    member a.ContentType with get() = contentType and set v = contentType <- v
    member a.IsByteArray with get() = isByteArray and set v = isByteArray <- v
    member a.OriginalEncoding with get() = originalEncoding and set v = originalEncoding <- v
    member a.Text with get() = text and set v = text <- v

// vim:ft=fs