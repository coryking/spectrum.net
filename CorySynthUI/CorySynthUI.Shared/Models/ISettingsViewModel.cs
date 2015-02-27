using CorySignalGenerator.Wave;
using System;
namespace CorySynthUI.Models
{
    public interface ISettingsViewModel
    {
        Device AudioRenderDevice { get; set; }
        Device MidiCaptureDevice { get; set; }
    }
}
