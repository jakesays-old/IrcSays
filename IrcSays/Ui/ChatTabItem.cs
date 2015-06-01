using System.Windows.Controls;

namespace IrcSays.Ui
{
	public class ChatTabItem : TabItem
	{
		private readonly ChatPage _content;

		public ChatPage Page
		{
			get { return _content; }
		}

		public ChatTabItem(ChatPage page)
		{
			_content = page;
			Content = page;
		}

		public void Dispose()
		{
			_content.Dispose();
		}
	}
}