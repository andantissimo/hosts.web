using System.IO;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Mvc.Formatters
{
    public class TextPlainInputFormatter : TextInputFormatter
    {
        public TextPlainInputFormatter()
        {
            SupportedMediaTypes.Add(MediaTypeNames.Text.Plain);
            SupportedEncodings.Add(UTF8EncodingWithoutBOM);
            SupportedEncodings.Add(UTF16EncodingLittleEndian);
        }

        public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context, Encoding encoding)
        {
            using var reader = new StreamReader(context.HttpContext.Request.Body, encoding);
            var body = await reader.ReadToEndAsync().ConfigureAwait(false);
            return await InputFormatterResult.SuccessAsync(body).ConfigureAwait(false);
        }
    }
}
