using System;
using System.Globalization;
using System.Threading.Tasks;
using ArcdpsLogManager.Logs;
using ArcdpsLogManager.Sections;
using ArcdpsLogManager.Uploaders;
using Eto.Drawing;
using Eto.Forms;
using ScratchEVTCParser.Model.Encounters;

namespace ArcdpsLogManager.Controls
{
	public class LogDetailPanel : DynamicLayout
	{
		public ImageProvider ImageProvider { get; }
		public DpsReportUploader DpsReportUploader { get; } = new DpsReportUploader();

		private LogData logData;

		private readonly Label nameLabel = new Label() { Font = Fonts.Sans(16, FontStyle.Bold) };
		private readonly Label resultLabel = new Label() { Font = Fonts.Sans(12) };
		private readonly Label timeLabel = new Label();
		private readonly Label durationLabel = new Label();
		private readonly GroupCompositionControl groupComposition;
		private readonly Label parseTimeLabel = new Label();
		private readonly Label parseStatusLabel = new Label();
		private readonly Button dpsReportUploadButton;
		private readonly TextBox dpsReportTextBox;
		private readonly Button dpsReportOpenButton;


		public LogData LogData
		{
			get => logData;
			set
			{
				SuspendLayout();
				logData = value;

				if (logData == null)
				{
					Visible = false;
					return;
				}

				Visible = true;

				nameLabel.Text = logData.EncounterName;

				string result;
				switch (logData.EncounterResult)
				{
					case EncounterResult.Success:
						result = "Success";
						break;
					case EncounterResult.Failure:
						result = "Failure";
						break;
					case EncounterResult.Unknown:
						result = "Unknown";
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}

				timeLabel.Text = logData.EncounterStartTime.ToLocalTime().DateTime.ToString(CultureInfo.CurrentCulture);

				double seconds = logData.EncounterDuration.TotalSeconds;
				string duration = $"{seconds / 60:0}m {seconds % 60:0.0}s";

				durationLabel.Text = duration;

				resultLabel.Text = $"{result} in {duration}";

				UpdateUploadStatus();

				ResumeLayout();
			}
		}

		private void UpdateUploadStatus()
		{
            parseTimeLabel.Text = $"{logData.ParseMilliseconds} ms";
            parseStatusLabel.Text = logData.ParsingStatus.ToString();

            groupComposition.Players = logData.Players;
            string uploadButtonText;
            switch (logData.DpsReportEIUpload.UploadState)
            {
	            case UploadState.NotUploaded:
		            uploadButtonText = "Upload to dps.report (EI)";
		            break;
	            case UploadState.Uploading:
		            uploadButtonText = "Uploading...";
		            break;
	            case UploadState.Uploaded:
		            uploadButtonText = "Reupload to dps.report (EI)";
		            break;
	            default:
		            throw new ArgumentOutOfRangeException();
            }

            dpsReportUploadButton.Text = uploadButtonText;
            dpsReportUploadButton.Enabled = logData.DpsReportEIUpload.UploadState != UploadState.Uploading;
            dpsReportTextBox.Text = logData.DpsReportEIUpload.Url ?? "";
            dpsReportOpenButton.Enabled = logData.DpsReportEIUpload.Url != null;
		}


		public LogDetailPanel(ImageProvider imageProvider)
		{
			ImageProvider = imageProvider;

			Padding = new Padding(10);
			Width = 300;
			Visible = false;

			groupComposition = new GroupCompositionControl(imageProvider);

			BeginVertical(spacing: new Size(0, 30));

			BeginVertical();
			Add(nameLabel);
			Add(resultLabel);
			EndVertical();
			BeginVertical(spacing: new Size(5, 5));
			AddRow("Encounter start", timeLabel);
			AddRow("Encounter duration", durationLabel);
			AddRow();
			EndVertical();
			BeginVertical();
			BeginHorizontal(true);
			Add(groupComposition);
			EndHorizontal();

			var debugSection = BeginVertical();
			var debugButton = new Button {Text = "Debug data"};
			BeginHorizontal();
			BeginVertical(xscale: true, spacing: new Size(5, 0));
			AddRow("Time spent parsing", parseTimeLabel);
			AddRow("Parsing status", parseStatusLabel);
			EndVertical();
			BeginVertical();
			AddRow(debugButton);
			AddRow(null);
			EndVertical();
			EndHorizontal();
			EndVertical();

			dpsReportUploadButton = new Button();
			dpsReportTextBox = new TextBox {ReadOnly = true};
			dpsReportOpenButton = new Button {Text = "Open"};

			BeginVertical(spacing: new Size(5, 5));
			BeginHorizontal();
			Add(dpsReportUploadButton);
			//Add(new Button {Text = "Upload to gw2raidar", Enabled = false}); // TODO: Implement
			EndHorizontal();
			BeginHorizontal();
			BeginVertical();
			BeginHorizontal();
			Add(dpsReportTextBox, true);
			Add(dpsReportOpenButton);
			EndHorizontal();
			EndVertical();
			//AddSeparateRow(new TextBox {Text = "", ReadOnly = true, Width = 80}, new Button {Text = "Open"});
			EndHorizontal();
			EndVertical();

			EndVertical();

			dpsReportUploadButton.Click += (sender, args) =>
			{
				Task.Run(() => UploadDpsReportEliteInsights(logData));
			};
			dpsReportOpenButton.Click += (sender, args) =>
			{
				System.Diagnostics.Process.Start(logData.DpsReportEIUpload.Url);
			};

			debugButton.Click += (sender, args) =>
			{
				var debugData = new DebugData {LogData = LogData};
				var dialog = new Form {Content = debugData, Width = 400, Title = "Debug data"};
				dialog.Show();
			};

			Settings.ShowDebugDataChanged += (sender, args) => { debugSection.Visible = Settings.ShowDebugData; };
			Shown += (sender, args) =>
			{
				// Assigning visibility in the constructor did not work
				debugSection.Visible = Settings.ShowDebugData;
			};
		}

		private async void UploadDpsReportEliteInsights(LogData logData)
		{
			// Keep in mind that a different log could be shown at this point.
			// For this reason we can only recheck the upload status instead of changing it directly.

			logData.DpsReportEIUpload.UploadState = UploadState.Uploading;
			Application.Instance.Invoke(UpdateUploadStatus);

			var url = await DpsReportUploader.UploadLogAsync(LogData);
			logData.DpsReportEIUpload.Url = url;
			logData.DpsReportEIUpload.UploadState = UploadState.Uploaded;
			Application.Instance.Invoke(UpdateUploadStatus);
		}
	}
}