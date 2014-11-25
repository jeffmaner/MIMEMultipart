﻿using MIMEMultipart;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UsageTest
{
	class GenerationTester
	{
		public static void TestGeneration()
		{
			TestComplexStructure();
			TestSimpleStructure();
		}

		private static void TestComplexStructure()
		{
			var contentIDBase = DateTime.Now.Ticks.ToString();

			var nodeA = new Attachment();

			var nodeA1 = new Attachment();
			nodeA1.ContentType = "image/jpg";
			nodeA1.IsByteArray = true;
			nodeA1.OriginalEncoding = "Base64";
			nodeA1.Bytes = File.ReadAllBytes(@"C:\Users\xyz\Pictures\JPEG_example_JPG_RIP_025.jpg");

			var nodeA2 = new Attachment();

			var nodeA2a = new Attachment();
			nodeA2a.ContentType = "Application/XML";
			nodeA2a.OriginalEncoding = "binary";
			nodeA2a.ContentID = contentIDBase + ".1";
			nodeA2a.Text = File.ReadAllText(@"C:\Users\xyz\Documents\Projects\rn\samplePart1.xml");

			var nodeA2b = new Attachment();
			nodeA2b.ContentType = "text/pdf";
			nodeA2b.IsByteArray = true;
			nodeA2b.OriginalEncoding = "Base64";
			nodeA2b.Bytes = File.ReadAllBytes(@"C:\Users\xyz\Documents\Projects\rn\Invoice Import - PIDX RNIF.PDF");

			nodeA2.ContentType = "multipart/related";
			nodeA2.Attachments = new[] { nodeA2a, nodeA2b };

			var nodeA3 = new Attachment();
			nodeA3.ContentType = "Application/XML";
			nodeA3.OriginalEncoding = "binary";
			nodeA3.ContentID = contentIDBase + ".2";
			nodeA3.Text = File.ReadAllText(@"C:\Users\xyz\Documents\Projects\rn\samplePart2.xml");

			nodeA.ContentType = "multipart/related";
			nodeA.Attachments = new[] { nodeA1, nodeA2, nodeA3 };

			var nodeB = new Attachment();

			var nodeB1 = new Attachment();
			nodeB1.ContentType = "image/jpg";
			nodeB1.IsByteArray = true;
			nodeB1.OriginalEncoding = "Base64";
			nodeB1.Bytes = File.ReadAllBytes(@"C:\Users\xyz\Pictures\mini.jpg");

			var nodeB2 = new Attachment();
			nodeB2.ContentType = "Application/XML";
			nodeB2.OriginalEncoding = "binary";
			nodeB2.ContentID = contentIDBase + ".3";
			nodeB2.Text = File.ReadAllText(@"C:\Users\xyz\Documents\Projects\rn\samplePart3.xml");

			nodeB.ContentType = "multipart/related";
			nodeB.Attachments = new[] { nodeB1, nodeB2 };

			var root = new Attachment();
			root.ContentType = "Multipart/related";
			root.Attachments = new[] { nodeA, nodeB };

			File.WriteAllText(@"C:\Users\xyz\Documents\Projects\rn\mime_generator.out", root.GenerateAttachmentString());
		}

		private static void TestSimpleStructure()
		{
			const string path = @"C:\Users\xyz\Documents\Projects\rn";

			var ns = new[] { 1, 2, 3, 4 };
			var ps = ns.Select(n => String.Format(@"{0}\samplePart{1}.xml", path, n));
			var files = ps.Select(p => File.ReadAllText(p));
			var nfs = ns.Zip(files, (n, s) => new Tuple<int, string>(n, s));
			var attachments = nfs.Select(x => new Attachment
			{
				ContentType = "Application/XML"
			,
				ContentID = DateTime.Now.Ticks.ToString() + "." + x.Item1.ToString()
			,
				OriginalEncoding = "binary"
			,
				Text = x.Item2
			});

			Console.WriteLine(CreateAttachment(attachments).GenerateAttachmentString());
			Console.ReadKey();
		}

		private static Attachment CreateAttachment(IEnumerable<Attachment> attachments)
		{
			var a = new Attachment();
			a.ContentType = "Multipart/related";
			a.Attachments = attachments;

			return a;
		}
	}
}
