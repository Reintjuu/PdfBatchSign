using CommandLine;
using iText.IO.Font.Constants;
using iText.IO.Image;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

		[Option('u', "pagenumber", HelpText = "The page to add the content to.", Default = 1, Required = false)]
		public int PageNumber { get; set; }

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

	class Program
	{
		static void Main(string[] args)
		{
			Parser.Default.ParseArguments<Options>(args)
				.WithParsed(o =>
				{
					if (o.PageNumber < 1)
					{
						Console.WriteLine("Provide a page number bigger than or equal to 1.");
						return;
					}

					if (o.AllowedImageHeight < 1)
					{
						Console.WriteLine("Provide an allowed image height bigger than or equal to 1.");
						return;
					}

					string path = GetAbsolutePath(null, o.Path);
					string[] files;
					if (o.Verbose)
					{
						Console.WriteLine($"Trying path: {path}.");
					}

					if (Directory.Exists(path))
					{
						files = Directory.GetFiles(path, "*.pdf");
						if (files.Length == 0)
						{
							Console.WriteLine("No PDF files have been found.");
							return;
						}
					}
					else
					{
						Console.WriteLine("Path not found.");
						return;
					}

					// Create the signed directory if it doesn't exist yet.
					Directory.CreateDirectory($"{path}/{o.SignedFolderName}");

					ImageData imageData;
					try
					{
						// Load the image from disk.
						imageData = ImageDataFactory.Create(o.Image);
					}
					catch
					{
						Console.WriteLine("An unexpected error has occurred while loading the image file.");
						return;
					}

					float imageHeight = imageData.GetHeight();
					float imageWidth = imageData.GetWidth();
					if (imageHeight == 0 || imageWidth == 0)
					{
						Console.WriteLine("Provide an image with resolutions bigger than 0.");
						return;
					}

					foreach (string file in files)
					{
						PdfDocument pdfDocument;
						try
						{
							// Modifying the PDF located at "source" and saving to "target".
							pdfDocument = new PdfDocument(new PdfReader(file), new PdfWriter($"{path}/{o.SignedFolderName}/{Path.GetFileName(file)}"));
						}
						catch
						{
							Console.WriteLine($"Failed to load {file}, skipping.");
							continue;
						}

						// Document to add layout elements: paragraphs, images etc
						Document document = new Document(pdfDocument);

						// Create and add the layout image object.
						Image image = new Image(imageData)
							.ScaleAbsolute(o.AllowedImageHeight / imageData.GetHeight() * imageData.GetWidth(), o.AllowedImageHeight)
							.SetFixedPosition(o.PageNumber, o.ImagePosition.ElementAtOrDefault(0), o.ImagePosition.ElementAtOrDefault(1));
						document.Add(image);

						// Create the date text.
						var paragraph = new Paragraph(!string.IsNullOrEmpty(o.Date) ? o.Date : DateTime.Now.ToShortDateString())
							.SetMargin(0)
							.SetMultipliedLeading(1)
							.SetFont(PdfFontFactory.CreateFont(StandardFonts.TIMES_ROMAN))
							.SetFontSize(10);
						// Add the date text to the document.
						document.ShowTextAligned(paragraph, o.DatePosition.ElementAtOrDefault(0), o.DatePosition.ElementAtOrDefault(1), o.PageNumber, TextAlignment.LEFT, VerticalAlignment.BOTTOM, 0);

						document.Close();

						if (o.Verbose)
						{
							Console.WriteLine($"Done signing: {file}");
						}
					}
				});
		}

		private static string GetAbsolutePath(string basePath, string path)
		{
			if (path == null)
			{
				return null;
			}

			// Quick way of getting current working directory, if the basepath is null, make sure we actually get a base path.
			basePath = (basePath == null) ? Path.GetFullPath(".") : GetAbsolutePath(null, basePath);

			string finalPath;
			// Specific for windows paths starting on \ - it needs the drive to be added.
			// It also adds possible cross-platform/Mono support.
			if (!Path.IsPathRooted(path) || "\\".Equals(Path.GetPathRoot(path)))
			{
				finalPath = path.StartsWith(Path.DirectorySeparatorChar.ToString())
					? Path.Combine(Path.GetPathRoot(basePath), path.TrimStart(Path.DirectorySeparatorChar))
					: Path.Combine(basePath, path);
			}
			else
			{
				finalPath = path;
			}

			// Resolve any internal "..\" to get the true full path.
			return Path.GetFullPath(finalPath);
		}
	}
}
