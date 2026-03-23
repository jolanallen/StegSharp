using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Stegano.UI;

public partial class Dashboard : UserControl
{
    public Dashboard()
    {
        InitializeComponent();


        var encoder = this.FindControl<ContentControl>("EncoderAffichage");
        var decoder = this.FindControl<ContentControl>("DecoderAffichage");
    
        if (encoder != null) encoder.Content = new Encoder();
        if (decoder != null) decoder.Content = new Decoder();
       
        
    }

}

    
    