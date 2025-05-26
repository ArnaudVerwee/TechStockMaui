using TechStockMaui.Controls;

namespace TechStockMaui.Views.MaterialManagements;

public partial class SignProductPage : ContentPage
{
    private int _productId;
    public SignProductPage(int productId)
	{
		InitializeComponent();
        _productId = productId;
    }

    private void OnStartDraw(object sender, TouchEventArgs e)
    {
        // D�but du dessin
    }

    private void OnDraw(object sender, TouchEventArgs e)
    {
        // En cours de dessin
    }

    private void OnEndDraw(object sender, TouchEventArgs e)
    {
        // Fin du dessin
    }
    private void OnClearClicked(object sender, EventArgs e)
    {
        // Ici tu peux remettre � z�ro les donn�es de dessin sur le SignaturePad
        if (SignatureDrawable.Instance != null)
        {
            SignatureDrawable.Instance.Clear();
            SignaturePad.Invalidate(); // Pour forcer le redessin de la vue
        }
    }

    private void OnSubmitClicked(object sender, EventArgs e)
    {
        // Ta logique pour g�rer le clic sur "Soumettre"
    }


}