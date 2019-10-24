using PdfSharp.Pdf;
using PdfSharp.Pdf.Content;
using PdfSharp.Pdf.Content.Objects;
using PdfSharp.Pdf.IO;
using System.Collections.Generic;
using System.Linq;

namespace DocumentCrossReference.Library.Importers
{
    public static class PdfImporter
    {
        public static DocumentTextIndex IndexPdfDocument(string pdfFilePath)
        {
            // Add Watermark
            using (var doc = PdfReader.Open(pdfFilePath, PdfDocumentOpenMode.ReadOnly))
            {
                var allTexts = new List<DocumentTextLocation>();

                var iPage = 0;
                foreach (var page in doc.Pages)
                {
                    var texts = page.ExtractText().ToList();
                    texts.ForEach(x =>
                    {
                        x.PageIndex = iPage;
                        x.DocumentFilePath = pdfFilePath;
                    });
                    allTexts.AddRange(texts);

                    iPage++;
                }

                doc.Close();

                return new DocumentTextIndex()
                {
                    TextEntries = allTexts.GroupBy(x => x.Text).ToDictionary(g => g.Key, g => g.ToList())
                };
            }
        }

        public static IEnumerable<DocumentTextLocation> ExtractText(this PdfPage page)
        {
            var content = ContentReader.ReadContent(page);
            var text = content.ExtractText();
            return text;
        }

        public static IEnumerable<DocumentTextLocation> ExtractText(this CObject cObject)
        {
            if (cObject is COperator)
            {
                var cOperator = cObject as COperator;
                if (cOperator.OpCode.Name == OpCodeName.Tj.ToString() ||
                    cOperator.OpCode.Name == OpCodeName.TJ.ToString())
                {
                    foreach (var cOperand in cOperator.Operands)
                    {
                        foreach (var txt in ExtractText(cOperand))
                        {
                            yield return txt;
                        }
                    }
                }
            }
            else if (cObject is CSequence)
            {
                var cSequence = cObject as CSequence;
                foreach (var element in cSequence)
                {
                    foreach (var txt in ExtractText(element))
                    {
                        yield return txt;
                    }
                }
            }
            else if (cObject is CString)
            {
                var cString = cObject as CString;
                yield return new DocumentTextLocation() { Text = cString.Value };
            }
        }
    }
}
