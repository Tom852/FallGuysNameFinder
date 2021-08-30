namespace Backend.Model
{
    public class OcrResult
    {
        public OcrResult(string text, float confidence)
        {
            Text = text;
            Confidence = confidence;
        }

        public string Text { get; set; }
        public float Confidence { get; set; }

        public bool HasText => !string.IsNullOrWhiteSpace(Text);
    }
}