using Microsoft.Maui.Graphics;

namespace TechStockMaui.Controls
{
    public class SignatureDrawable : IDrawable
    {
        public static SignatureDrawable Instance { get; } = new SignatureDrawable();

        private PathF _path = new PathF();

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            canvas.StrokeColor = Colors.Black;
            canvas.StrokeSize = 2;
            canvas.DrawPath(_path);
        }

        public void Start(float x, float y)
        {
            _path.MoveTo(x, y);
        }

        public void LineTo(float x, float y)
        {
            _path.LineTo(x, y);
        }

        public void End()
        {
            // Optionnel : logique à la fin du dessin
        }

        public void Clear()
        {
            _path = new PathF();
        }
    }
}
