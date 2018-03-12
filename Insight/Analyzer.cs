﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Insight.Analyzers;
using Insight.Builder;
using Insight.Dto;
using Insight.Metrics;
using Insight.Shared.Model;
using Insight.Shared.VersionControl;

using Newtonsoft.Json;

using Visualization.Controls;
using Visualization.Controls.Bitmap;

namespace Insight
{
    public sealed class Analyzer
    {
        private Dictionary<string, Contribution> _contributions;
        private ChangeSetHistory _history;
        private Dictionary<string, LinesOfCode> _metrics;

        public Analyzer(Project project)
        {
            Project = project;
        }

        public List<WarningMessage> Warnings { get; private set; }

        private Project Project { get; }


        public List<Coupling> AnalyzeChangeCoupling()
        {
            LoadHistory();

            // Pair wise couplings
            var tmp = new ChangeCouplingAnalyzer();
            var couplings = tmp.CalculateChangeCouplings(_history, Project.Filter);
            var sortedCouplings = couplings.OrderByDescending(coupling => coupling.Degree).ToList();
            Csv.Write(Path.Combine(Project.Cache, "change_couplings.csv"), sortedCouplings);

            // Same with classified folders
            var classifiedCouplings = tmp.CalculateChangeCouplings(_history, localPath => { return ClassifyDirectory(localPath); });
            Csv.Write(Path.Combine(Project.Cache, "classified_change_couplings.csv"), classifiedCouplings);

            return sortedCouplings;
        }

        public HierarchicalDataContext AnalyzeCodeAge()
        {
            LoadHistory();
            LoadMetrics();

            // Get summary of all files
            var summary = _history.GetArtifactSummary(Project.Filter, new HashSet<string>(_metrics.Keys));

            var builder = new CodeAgeBuilder();
            var hierarchicalData = builder.Build(summary, _metrics);
            return new HierarchicalDataContext(hierarchicalData);
        }

        public HierarchicalDataContext AnalyzeHotspots()
        {
            LoadHistory();
            LoadMetrics();

            // Get summary of all files
            var summary = _history.GetArtifactSummary(Project.Filter, new HashSet<string>(_metrics.Keys));

            var builder = new HotspotBuilder();
            var hierarchicalData = builder.Build(summary, _metrics);
            return new HierarchicalDataContext(hierarchicalData);
        }

        public HierarchicalDataContext AnalyzeKnowledge()
        {
            LoadHistory();
            LoadMetrics();
            LoadContributions();

            var summary = _history.GetArtifactSummary(Project.Filter, new HashSet<string>(_metrics.Keys));
            var fileToMainDeveloper = _contributions.ToDictionary(pair => pair.Key, pair => pair.Value.GetMainDeveloper());

            // Assign a color to each developer
            var mainDevelopers = fileToMainDeveloper.Select(pair => pair.Value.Developer).Distinct();
            var scheme = new ColorScheme(mainDevelopers.ToArray());

            // Build the knowledge data
            var builder = new KnowledgeBuilder();
            var hierarchicalData = builder.Build(summary, _metrics, fileToMainDeveloper);

            return new HierarchicalDataContext(hierarchicalData, scheme);
        }

        public List<TrendData> AnalyzeTrend(string localFile)
        {
            var trend = new List<TrendData>();

            var svnProvider = Project.CreateProvider();

            // Svn log on this file to get all revisions
            var fileHistory = svnProvider.ExportFileHistory(localFile);

            // For each file we need to calculate the metrics
            var provider = new CodeMetrics();

            foreach (var file in fileHistory)
            {
                var fileInfo = new FileInfo(file.CachePath);
                var loc = provider.CalculateLinesOfCode(fileInfo);
                var invertedSpace = provider.CalculateInvertedSpaceMetric(fileInfo);
                trend.Add(new TrendData { Date = file.Date, Loc = loc, InvertedSpace = invertedSpace });
            }

            return trend;
        }

        /// <summary>
        /// If we call this in context of a knowledge map we reuse existing colors colors and simply add
        /// missing developers/colors. You can optionally add a uninitialized (default ctor) ColorScheme.
        /// </summary>
        public string AnalyzeWorkOnSingleFile(string fileName, ColorScheme colorScheme)
        {
            Debug.Assert(colorScheme != null);
            var provider = Project.CreateProvider();
            var workByDeveloper = provider.CalculateDeveloperWork(new Artifact { LocalPath = fileName });

            var bitmap = new FractionBitmap();

            var fi = new FileInfo(fileName);
            var path = Path.Combine(Project.Cache, fi.Name) + ".bmp";

            InitColorMappingForWork(colorScheme, workByDeveloper);

            bitmap.Create(path, workByDeveloper, colorScheme, true);

            return path;
        }

        public List<DataGridFriendlyComment> ExportComments()
        {
            /*
              R Code
              library(tm)
              library(wordcloud)

              comments = read.csv("d:\\comments.csv", stringsAsFactors=FALSE)
              names(comments) = c("comment")

              corpus = Corpus(VectorSource(comments[,1]))
              corpus = tm_map(corpus, tolower)
              #corpus = tm_map(corpus, PlainTextDocument)
              corpus = tm_map(corpus, removePunctuation)
              corpus = tm_map(corpus, removeWords, stopwords("english"))
              frequencies = DocumentTermMatrix(corpus)
              sparse = removeSparseTerms(frequencies, 0.99)
              all = as.data.frame(as.matrix(sparse))

              wordcloud(colnames(all), colSums(all))
          */

            LoadHistory();
            var result = new List<DataGridFriendlyComment>();
            foreach (var cs in _history.ChangeSets)
            {
                result.Add(new DataGridFriendlyComment
                           {
                                   Committer = cs.Committer,
                                   Comment = cs.Comment
                           });
            }

            Csv.Write(Path.Combine(Project.Cache, "comments.csv"), result);
            return result;
        }

        public List<DataGridFriendlyArtifact> ExportSummary()
        {
            LoadHistory();
            LoadMetrics();

            var summary = _history.GetArtifactSummary(Project.Filter, new HashSet<string>(_metrics.Keys));

            var gridData = new List<DataGridFriendlyArtifact>();
            foreach (var artifact in summary)
            {
                var metricKey = artifact.LocalPath.ToLowerInvariant();
                var loc = _metrics.ContainsKey(metricKey) ? _metrics[metricKey].Code : 0;
                gridData.Add(
                             new DataGridFriendlyArtifact
                             {
                                     LocalPath = artifact.LocalPath,
                                     Revision = artifact.Revision,
                                     Commits = artifact.Commits,
                                     Committers = artifact.Committers.Count,
                                     LOC = loc,
                                     WorkItems = artifact.WorkItems.Count,
                                     CodeAge_Days = (DateTime.Now - artifact.Date).Days
                             });
            }

            Csv.Write(Path.Combine(Project.Cache, "summary.csv"), gridData);
            return gridData;
        }

        public void UpdateCache(bool includeContributions)
        {
            // Note: You should have the latest code locally such that history and metrics match!
            // Update svn history
            var svnProvider = Project.CreateProvider();
            svnProvider.UpdateCache();

            // Update code metrics
            var metricProvider = new MetricProvider(Project.ProjectBase, Project.Cache, Project.GetNormalizedFileExtensions());
            metricProvider.UpdateCache();

            File.Delete(GetPathToContributionFile());
            if (includeContributions)
            {
                // Update contributions. This takes a long time. Not useful for svn.
                UpdateContributions();
            }
        }

        internal void Clear()
        {
            _history = null;
            _metrics = null;
            _contributions = null;
        }

        private static string ClassifyDirectory(string localPath)
        {
            // Classify different source code folders

            // THIS IS AN EXAMPLE
            if (localPath.Contains("UnitTest"))
            {
                return "Test";
            }

            if (localPath.Contains("UI"))
            {
                return "UserInterface";
            }

            if (localPath.Contains("bla\\bla\\bla"))
            {
                return "bla";
            }

            return string.Empty;
        }

        private Dictionary<string, Contribution> CalculateContributionsParallel(List<Artifact> summary)
        {
            // Calculate main developer for each file
            var fileToContribution = new ConcurrentDictionary<string, Contribution>();

            Parallel.ForEach(summary, new ParallelOptions { MaxDegreeOfParallelism = 4 },
                             artifact =>
                             {
                                 var provider = Project.CreateProvider();
                                 var work = provider.CalculateDeveloperWork(artifact);
                                 var contribution = new Contribution(work);

                                 fileToContribution.TryAdd(artifact.LocalPath, contribution);
                             });

            return fileToContribution.ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        private string GetPathToContributionFile()
        {
            return Path.Combine(Project.Cache, "contribution_analysis.json");
        }

        private void InitColorMappingForWork(ColorScheme colorMapping, Dictionary<string, uint> workByDeveloper)
        {
            // order such that same developers get same colors regardless of order.
            foreach (var developer in workByDeveloper.Keys.OrderBy(x => x))
            {
                colorMapping.AddColorKey(developer);
            }
        }

        private void LoadContributions()
        {
            if (_contributions == null)
            {
                if (File.Exists(GetPathToContributionFile()) == false)
                {
                    throw new Exception($"The file '{GetPathToContributionFile()}' was not found. Please click Sync to create it.");
                }

                var input = File.ReadAllText(GetPathToContributionFile(), Encoding.UTF8);
                _contributions = JsonConvert.DeserializeObject<Dictionary<string, Contribution>>(input);
            }
        }

        private void LoadHistory()
        {
            if (_history == null)
            {
                var provider = Project.CreateProvider();
                _history = provider.QueryChangeSetHistory();
                Warnings = provider.Warnings;

                // Remove all items that are deleted now.
                _history.CleanupHistory();
            }
        }

        private void LoadMetrics()
        {
            // Get code metrics (all files from the cache!)
            if (_metrics == null)
            {
                var metricProvider = new MetricProvider(Project.ProjectBase, Project.Cache, Project.GetNormalizedFileExtensions());
                _metrics = metricProvider.QueryCodeMetrics();
            }
        }

        private void UpdateContributions()
        {
            LoadHistory();
            LoadMetrics();

            var summary = _history.GetArtifactSummary(Project.Filter, new HashSet<string>(_metrics.Keys));
            _contributions = CalculateContributionsParallel(summary);

            var json = JsonConvert.SerializeObject(_contributions);
            var path = GetPathToContributionFile();
            File.WriteAllText(path, json, Encoding.UTF8);
        }
    }
}