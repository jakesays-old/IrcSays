using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;

namespace IrcSays.Ui
{
	[TemplatePart(Name = "PART_ChatPresenter", Type = typeof (ChatPresenter))]
	public class ChatBox : ChatBoxBase
	{
		private ChatPresenter _presenter;

		public bool IsAutoScrolling
		{
			get { return Presenter.IsAutoScrolling; }
		}

		public ChatPresenter Presenter
		{
			get { return _presenter; }
		}

		public ChatBox()
		{
			DefaultStyleKey = typeof (ChatBox);
			ApplyTemplate();
		}

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			_presenter = GetTemplateChild("PART_ChatPresenter") as ChatPresenter;
			if (Presenter == null)
			{
				throw new Exception("Missing template part.");
			}
		}

		public void AppendBulkLines(IEnumerable<ChatLine> lines)
		{
			Presenter.AppendBulkLines(lines);
		}

		public void AppendLine(ChatLine line)
		{
			Presenter.AppendLine(line);
		}

		public void Clear()
		{
			Presenter.Clear();
		}

		public void PageUp()
		{
			Presenter.PageUp();
		}

		public void PageDown()
		{
			Presenter.PageDown();
		}

		public void MouseWheelUp()
		{
			Presenter.MouseWheelUp();
		}

		public void MouseWheelDown()
		{
			Presenter.MouseWheelDown();
		}

		public void Search(Regex pattern, SearchDirection dir)
		{
			Presenter.Search(pattern, dir);
		}

		public void ClearSearch()
		{
			Presenter.ClearSearch();
		}

		public void PurgeMessages(string nick)
		{
			Presenter.PurgeMessages(nick);
		}
	}
}