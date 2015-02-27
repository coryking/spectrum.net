using System;
namespace CorySynthUI.Models
{
    public interface ISettingsViewModel
    {
        String AudioRenderDeviceId { get; set; }
        String MidiCaptureDeviceId { get; set; }
    }
}
