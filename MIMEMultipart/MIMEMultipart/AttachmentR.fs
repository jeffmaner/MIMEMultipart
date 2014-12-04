namespace MIMEMultipart

module internal AttachmentR = // Exposing AttachmentR.
  type internal AttachmentR = { ContentType      : string
                              ; ContentID        : string
                              ; OriginalEncoding : string
                              ; IsByteArray      : bool
                              ; Bytes            : byte []
                              ; Text             : string
                              ; Attachments      : AttachmentR seq }

// vim:ft=fs