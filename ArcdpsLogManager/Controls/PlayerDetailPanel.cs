using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using ArcdpsLogManager.Annotations;
using ArcdpsLogManager.Logs;
using ArcdpsLogManager.Sections;
using Eto.Drawing;
using Eto.Forms;
using ScratchEVTCParser.Model.Agents;

namespace ArcdpsLogManager.Controls
{
	public class PlayerDetailPanel : DynamicLayout, INotifyPropertyChanged
	{
		private const string NullAccountName = "-"; // Account names start with :, so this should never appear.

		private PlayerData playerData = new PlayerData(NullAccountName, new LogData[0]);
		public ImageProvider ImageProvider { get; }

		public PlayerData PlayerData
		{
			get => playerData;
			set
			{
				if (value == null)
				{
                    value = new PlayerData(NullAccountName, new LogData[0]);
				}

				if (Equals(value, playerData)) return;
				playerData = value;
				OnPropertyChanged();
			}
		}

		public PlayerDetailPanel(ImageProvider imageProvider)
		{
			ImageProvider = imageProvider;

			Padding = new Padding(10);
			Width = 300;
			Visible = false;

			var accountName = new Label()
			{
				Font = Fonts.Sans(16, FontStyle.Bold)
			};
			PropertyChanged += (sender, args) =>
			{
				if (args.PropertyName != nameof(PlayerData)) return;

				accountName.Text = PlayerData?.AccountName?.Substring(1) ?? "";
			};

			var logCountLabel = new Label()
			{
				Font = Fonts.Sans(12)
			};
			PropertyChanged += (sender, args) =>
			{
				if (args.PropertyName != nameof(PlayerData)) return;
				if (PlayerData?.Logs == null) return;

				logCountLabel.Text = $"Appears in {PlayerData.Logs.Count} {(PlayerData.Logs.Count == 1 ? "log" : "logs")}";
			};

			var knownCharacters = new DynamicLayout();
			PropertyChanged += (sender, args) =>
			{
				if (args.PropertyName != nameof(PlayerData)) return;

				Visible = PlayerData.AccountName != NullAccountName;

				var characters = new Dictionary<string, (Profession profession, int count)>();
				foreach (var log in PlayerData.Logs)
				{
					var player = log.Players.First(x => x.AccountName == PlayerData.AccountName);

					if (!characters.ContainsKey(player.Name))
					{
						characters[player.Name] = (player.Profession, 0);
					}

					characters[player.Name] = (player.Profession, characters[player.Name].count + 1);
				}


				knownCharacters.Clear();
				knownCharacters.BeginHorizontal();
				knownCharacters.BeginVertical(spacing: new Size(5, 5));

				knownCharacters.AddRow("", "Character name", "Log count", null);
				foreach (var character in characters.OrderByDescending(x => x.Value.count))
				{
					knownCharacters.AddRow(
						ImageProvider.GetTinyProfessionIcon(character.Value.profession),
						character.Key,
						$"{character.Value.count}",
						null
					);
				}

				knownCharacters.AddRow(null);
				knownCharacters.EndVertical();
				knownCharacters.Add(null);
				knownCharacters.EndHorizontal();
				knownCharacters.Create();
			};

			BeginVertical(spacing: new Size(0, 30));

			BeginVertical();
			Add(accountName);
			Add(logCountLabel);
			EndVertical();
			BeginVertical(yscale: true);
			Add(knownCharacters);
			EndVertical();

			var logListButton = new Button {Text = "Show logs with this player"};
			logListButton.Click += (sender, args) =>
			{
				var form = new Form
				{
					Content = new LogList(ImageProvider) { DataStore = PlayerData.Logs.ToArray()},
					Width = 900,
					Height = 700,
					Title = $"arcdps Log Manager: logs with {PlayerData.AccountName.Substring(1)}"
				};
				form.Show();
			};
			BeginVertical();
			Add(logListButton);
			EndVertical();

			EndVertical();
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}