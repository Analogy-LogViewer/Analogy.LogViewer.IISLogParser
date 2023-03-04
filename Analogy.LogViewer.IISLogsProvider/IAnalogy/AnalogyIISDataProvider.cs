﻿using Analogy.Interfaces.DataTypes;
using Analogy.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Analogy.LogViewer.Template.Managers;

namespace Analogy.LogViewer.IISLogsProvider.IAnalogy
{
    public class AnalogyIISDataProvider : Analogy.LogViewer.Template.OfflineDataProvider
    {
        public override string? OptionalTitle { get; set; } = "Analogy IIS Log Parser";

        public override Guid Id { get; set; } = new Guid("44688C02-3156-45B1-B916-08DB96BCD358");
        public override Image? LargeImage { get; set; } = null;
        public override Image? SmallImage { get; set; } = null;
        public override string FileOpenDialogFilters { get; set; } = "IIS log files|u_ex*.log";
        public override IEnumerable<string> SupportFormats { get; set; } = new[] { "u_ex*.log" };
        private ILogParserSettings LogParserSettings { get; set; }
        private IISFileParser IISFileParser { get; set; }
        private string iisFileSetting { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Analogy.LogViewer", "AnalogyIISSettings.json");

        public override Task InitializeDataProvider(IAnalogyLogger logger)
        {
            LogManager.Instance.SetLogger(logger);
            if (File.Exists(iisFileSetting))
            {
                try
                {
                    LogParserSettings = JsonConvert.DeserializeObject<LogParserSettings>(iisFileSetting);
                }
                catch (Exception)
                {
                    LogParserSettings = new LogParserSettings();
                    LogParserSettings.IsConfigured = true;
                    LogParserSettings.SupportedFilesExtensions = new List<string> { "u_ex*.log" };
                }
            }
            else
            {
                LogParserSettings = new LogParserSettings();
                LogParserSettings.IsConfigured = true;
                LogParserSettings.SupportedFilesExtensions = new List<string> { "u_ex*.log" };

            }
            IISFileParser = new IISFileParser(LogParserSettings);
            return base.InitializeDataProvider(logger);
        }

        public override async Task<IEnumerable<IAnalogyLogMessage>> Process(string fileName, CancellationToken token, ILogMessageCreatedHandler messagesHandler)
        {
            if (CanOpenFile(fileName))
            {
                return await IISFileParser.Process(fileName, token, messagesHandler);
            }

            return new List<IAnalogyLogMessage>(0);

        }


        public override bool CanOpenFile(string fileName) => LogParserSettings.CanOpenFile(fileName);

        protected override List<FileInfo> GetSupportedFilesInternal(DirectoryInfo dirInfo, bool recursive)
        {
            List<FileInfo> files = dirInfo.GetFiles("u_ex*.log").ToList();
            if (!recursive)
            {
                return files;
            }

            try
            {
                foreach (DirectoryInfo dir in dirInfo.GetDirectories())
                {
                    files.AddRange(GetSupportedFilesInternal(dir, true));
                }
            }
            catch (Exception)
            {
                return files;
            }

            return files;
        }
    }

}
