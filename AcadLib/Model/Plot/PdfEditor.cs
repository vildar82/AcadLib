using System.Collections.Generic;

namespace AcadLib.Plot
{
    using System.IO;
    using iTextSharp.text;
    using iTextSharp.text.pdf;

    public class PdfEditor
    {
        public void AddSheet(string pdfFile, string addSheetFile, int numberInSheetFile, int numberInPdfFile, string destFile)
        {
            var sheet = new PdfReader(addSheetFile);
            var pdf = new PdfReader(pdfFile);
            var doc = new Document();
            var copier = new PdfCopy(doc, new FileStream(destFile, FileMode.Create));
            doc.Open();
            var pdfLabels = PdfPageLabels.GetPageLabels(pdf);
            if (pdfLabels?.Length != pdf.NumberOfPages)
                pdfLabels = null;
            var sheetLabel = PdfPageLabels.GetPageLabels(sheet);
            if (sheetLabel?.Length != sheet.NumberOfPages)
                sheetLabel = null;
            var labels = new PdfPageLabels();
            for (var i = 1; i < numberInPdfFile; i++)
            {
                var page = copier.GetImportedPage(pdf, i);
                copier.AddPage(page);
                if (pdfLabels != null)
                {
                    var label = pdfLabels[i - 1];
                    labels.AddPageLabel(i, PdfPageLabels.EMPTY, label);
                }
            }

            copier.AddPage(copier.GetImportedPage(sheet, numberInSheetFile));
            if (sheetLabel != null)
                labels.AddPageLabel(numberInPdfFile, PdfPageLabels.EMPTY, sheetLabel[numberInSheetFile - 1]);
            else
                labels.AddPageLabel(numberInPdfFile, PdfPageLabels.EMPTY, "бланк");
            for (var i = numberInPdfFile; i < pdf.NumberOfPages + 1; i++)
            {
                copier.AddPage(copier.GetImportedPage(pdf, i));
                if (pdfLabels != null)
                {
                    var label = pdfLabels[i - 1];
                    labels.AddPageLabel(i + 1, PdfPageLabels.EMPTY, label);
                }
            }

            copier.PageLabels = labels;
            doc.Close();
            copier.Close();
            pdf.Close();
            sheet.Close();
        }
    }
}