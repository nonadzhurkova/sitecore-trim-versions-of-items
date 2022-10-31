public class TrimOlderVersions
    {
        public bool Enabled { get; set; }
        public int MaxVersions { get; set; }
        public string Database { get; set; }
        public string RootItem { get; set; }

        public void Run()
        {
            if (Enabled)
            {
                Sitecore.Diagnostics.Log.Info("Trimming older versions of items starting", this);
                // Default values.
                MaxVersions = (MaxVersions < 1) ? 10 : MaxVersions;
                Database = (string.IsNullOrWhiteSpace(Database)) ? "master" : Database;

                // Get the database.
                var database = Sitecore.Configuration.Factory.GetDatabase(Database);

                if (database != null)
                {
                    var rootItem = database.GetItem(RootItem);

                    if (rootItem == null)
                    {
                        Sitecore.Diagnostics.Log.Error("Root item not found", this);
                        return;
                    }
                    Iterate(rootItem);
                    Sitecore.Diagnostics.Log.Info("Trimming versions complete", this);
                }
                else
                {
                    // Log.
                    Sitecore.Diagnostics.Log.Warn(string.Format("{0}: Failed to run. Database \"{1}\" is null.", this, Database), this);
                }
            }
            else
            {
                // Log.
                Sitecore.Diagnostics.Log.Info(string.Format("{0}: Task disabled.", this), this);
            }
        }

        protected void Iterate(Item item)
        {
            if (item != null)
            {
                // Get the version count of the item.
                var versionCount = item.Versions.GetVersionNumbers().Length;
                var latestVersion = item.Versions.GetLatestVersion(item.Language).Version.Number;

                // Don't bother looping if they're aren't enough versions.
                if (versionCount > MaxVersions)
                {
                    // Get all the versions we can archive.
                    var versions = item.Versions.GetVersions().Where(i => i.Version.Number <= (latestVersion - MaxVersions + 1) && i.Version.Number!=1);

                    foreach (var version in versions)
                    {
                        var archive = Sitecore.Data.Archiving.ArchiveManager.GetArchive("archive", Sitecore.Configuration.Factory.GetDatabase(Database));
                        if (archive != null)
                            archive.ArchiveVersion(version);
                    }
                }

                // recursive call for children
                foreach (Item child in item.Children)
                {
                    Iterate(child);
                }
            }
        }
    }