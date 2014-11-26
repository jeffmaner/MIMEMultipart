using MIMEMultipart;
using System;
using System.Configuration;
using System.IO;
using System.Linq;

namespace UsageTest
{
	class ParseTester
	{
		public static void TestParse()
		{
			var file = File.OpenText(ConfigurationManager.AppSettings["ParserTesterInputComplex"]);
			var xs = Attachment.ReadStreamWithHeaders(file);

			foreach (var x in xs)
				SaveAttachment(x);
		}

		private static void SaveAttachment(Attachment a)
		{
			var p = ConfigurationManager.AppSettings["ParserTesterOutput"];
			Func<Attachment, string> fileName = x =>
				Path.Combine(p, Path.ChangeExtension(Path.GetRandomFileName(),
													  FileExtension(x)));

			if (HasAttachments(a))
				foreach (var x in a.Attachments)
					SaveAttachment(x);
			else
				if (a.IsByteArray)
					File.WriteAllBytes(fileName(a), a.Bytes);
				else if (HasBody(a))
					File.WriteAllText(fileName(a), a.Text);
				// else there's nothing to write.
		}

		private static string FileExtension(Attachment a)
		{
			return a.ContentType.Split(new[] { '/' })[1];
		}

		private static bool HasAttachments(Attachment a)
		{
			return a.Attachments != null && a.Attachments.Any();
		}

		private static bool HasBody(Attachment a)
		{
			return !String.IsNullOrEmpty(a.Text);
		}
	}
}
