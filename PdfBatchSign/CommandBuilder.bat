@echo off
SETLOCAL
SET result=PdfBatchSign.exe


SET /p p=Path with PDF files to process (Required): 
if defined p SET result=%result% -p %p%

SET /p signature=Path to the image of the signature which will be added to the PDF files (Required): 
if defined signature SET result=%result% -s %signature%

SET /p name=Output folder name (Default: Signed): 
if defined name SET result=%result% -n %name%

SET /p d=The date to add to the PDF file, (Default: today's date): 
if defined d SET result=%result% -d %d%

SET /p pagenumber=The page to add the content to (Default: 1): 
if defined pagenumber SET result=%result% -u %pagenumber%

SET /p signatureposition=The X- and Y-position of the image, relative to the left-bottom of the page. Separated by a comma (Default: 150,430): 
if defined signatureposition SET result=%result% -i %signatureposition%

SET /p dateposition=The X- and Y-position of the date, relative to the left-bottom of the page. Separated by a comma (Default: 150,412): 
if defined dateposition SET result=%result% -j %dateposition%

SET /p allowedimageheight=The maximum height of the signature image, the image will be scaled accordingly (Default: 30): 
if defined allowedimageheight SET result=%result% -h %allowedimageheight%


:start
SET choice=
SET /p choice=Verbose output? [Y/n]: 
IF NOT '%choice%'=='' SET choice=%choice:~0,1%
IF '%choice%'=='Y' GOTO yes
IF '%choice%'=='y' GOTO yes
IF '%choice%'=='' GOTO yes
IF '%choice%'=='N' GOTO execute
IF '%choice%'=='n' GOTO execute
ECHO "%choice%" is not valid
ECHO.
GOTO start

:yes
SET result=%result% -v
GOTO execute

:execute
echo Executing command: %result%
%result%
pause
exit