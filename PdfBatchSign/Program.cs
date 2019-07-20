using CommandLine;
using iText.IO.Font.Constants;
using iText.IO.Image;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using System;
using System.IO;
using System.Linq;

namespace PdfBatchSign
{
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

						// Fixes the page rotation bug. The content that will be added now always matches the originally 
						// viewed orientation (not the orientation which has been set in the document's properties).
						pdfDocument.GetPage(o.PageNumber).SetIgnorePageRotationForContent(true);

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
