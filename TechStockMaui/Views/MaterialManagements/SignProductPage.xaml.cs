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
        
    }

    private void OnDraw(object sender, TouchEventArgs e)
    {
        
    }

    private void OnEndDraw(object sender, TouchEventArgs e)
    {
        
    }
    private void OnClearClicked(object sender, EventArgs e)
    {
       
        if (SignatureDrawable.Instance != null)
        {
            SignatureDrawable.Instance.Clear();
            SignaturePad.Invalidate(); 
        }
    }

    private void OnSubmitClicked(object sender, EventArgs e)
    {
      
    }


}