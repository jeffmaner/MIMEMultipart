﻿namespace MIMEMultipart

module Ancillary = // Exposing (/=) and nl.
  let (/=) = (<>) // A la Haskell. I think it's prettier.
  let nl = "\r\n"

