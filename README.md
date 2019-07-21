# PdfBatchSign
A batch tool that adds images and dates to existing PDF files.

## Usage
```
$ PdfBatchSign.exe -p . -s C:\signature.jpg
```
or

```
$ PdfBatchSign.exe -p "C:\Long Path\To\Pdfs" -s C:\signature.png
```

The most extensive way to call the tool is by using a command like:
```
$ PdfBatchSign.exe -p C:\Pdfs -s C:\signature.png -n output -d 20-6-2019 -i 100,200 -j 200,150 -h 20 -v
```

## Command builder
For additional convenience, a command builder has been added. Just open the `CommandBuilder.bat` file. 
This script asks questions based on the tool's input and eventually executes the command.

## More options
```bash
PdfBatchSign.exe --help
```

provides more information:

```
PdfBatchSign 1.0.0.0
Copyright c  2019

  -p, --path                  Required. Path with PDF files to process.

  -s, --signature             Required. Path to the image of the signature which will be added to the PDF files.

  -n, --name                  (Default: Signed) Output folder name.

  -d, --date                  The date to add to the PDF file, defaults to today's date.

  -i, --signatureposition     (Default: 150 430) The X- and Y-position of the image, relative to the left-bottom of the
                              page. Separated by a comma (e.g. 150,430).

  -j, --dateposition          (Default: 150 412) The X- and Y-position of the date, relative to the left-bottom of the
                              page. Separated by a comma (e.g. 150,412).

  -h, --allowedimageheight    (Default: 30) The maximum height of the signature image, the image will be scaled
                              accordingly.

  -v, --verbose               Set output to verbose messages.

  --help                      Display this help screen.

  --version                   Display version information.
```