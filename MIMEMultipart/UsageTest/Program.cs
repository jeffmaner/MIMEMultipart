using MIMEMultipart;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace UsageTest
{
	class Program
	{
		static void Main(string[] args)
		{
			GenerationTester.TestGeneration();
			ParseTester.TestParse();
		}
	}
}
