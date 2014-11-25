namespace MIMEMultipart

module AttachmentR = // Exposing AttachmentR.
  type AttachmentR = { ContentType      : string
                     ; ContentID        : string
                     ; OriginalEncoding : string
                     ; IsByteArray      : bool
                     ; Bytes            : byte []
                     ; Text             : string
                     ; Attachments      : AttachmentR seq }

// vim:ft=fs