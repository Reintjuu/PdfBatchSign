using CommandLine;
using System.Collections.Generic;

namespace PdfBatchSign
{
	public class Options
	{
		[Option('p', "path", HelpText = "Path with PDF files to process.", Required = true)]
		public string Path { get; set; }

		[Option('s', "signature", HelpText = "Path to the image of the signature which will be added to the PDF files.", Required = true)]
		public string Image { get; set; }

		[Option('n', "name", HelpText = "Output folder name.", Default = "Signed", Required = false)]
		public string SignedFolderName { get; set; }

		[Option('d', "date", HelpText = "The date to add to the PDF file, defaults to today's date.", Required = false)]
		public string Date { get; set; }

		[Option('i', "signatureposition",
			HelpText = "The X- and Y-position of the image, relative to the left-bottom of the page. Separated by a comma (e.g. 150,430).",
			Min = 2, Max = 2, Required = false, Default = new int[] { 150, 430 }, Separator = ',')]
		public IEnumerable<int> ImagePosition { get; set; }

		[Option('j', "dateposition",
			HelpText = "The X- and Y-position of the date, relative to the left-bottom of the page. Separated by a comma (e.g. 150,412).",
			Min = 2, Max = 2, Required = false, Default = new int[] { 150, 412 }, Separator = ',')]
		public IEnumerable<int> DatePosition { get; set; }

		[Option('h', "allowedimageheight",
			HelpText = "The maximum height of the signature image, the image will be scaled accordingly.",
			Default = 30, Required = false)]
		public int AllowedImageHeight { get; set; }

		[Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
		public bool Verbose { get; set; }
	}
}
