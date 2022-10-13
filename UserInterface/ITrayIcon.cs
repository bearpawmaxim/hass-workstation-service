using System;
using System.Windows.Input;

namespace MangaReader.Avalonia.Platform
{
	public interface ITrayIcon : IDisposable
	{
		ICommand DoubleClickCommand { get; set; }

		ICommand BalloonClickedCommand { get; set; }

		void SetIcon();

		void ShowBalloon(string text, object state);
	}
}