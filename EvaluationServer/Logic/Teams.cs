using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace VitretTool.EvaluationServer {
    class Teams {

        private StreamWriter mStreamWriter;
        private Dictionary<string, Team> mTeams;
        private VBSTasks.KeyframeEvaluator mEvaluator;

        public delegate void NewTeamAddedHandler(Team team);
        public event NewTeamAddedHandler NewTeamAdded;

        public Teams(VBSTasks.KeyframeEvaluator evaluator, NewTeamAddedHandler ntaHandler) {
            mEvaluator = evaluator;
            mTeams = new Dictionary<string, Team>();
            NewTeamAdded += ntaHandler;

            if (File.Exists("Teams/registered.txt")) {
                RestoreTeams();
            }
            mStreamWriter = new StreamWriter("Teams/registered.txt", append: true);
            mStreamWriter.AutoFlush = true;
        }

        public Team this[string index] {
            get {
                lock(mTeams) {
                    if (!mTeams.ContainsKey(index)) return null;
                    return mTeams[index];
                }
            }
        }

        public Team CreateNewTeam(string name) {
            long id = DateTime.Now.Ticks;
            Team t;

            lock (mTeams) {
                if (mTeams.ContainsKey(name)) return null;

                t = new Team(id, name, mEvaluator, ColorHelper.GetPredefiniedColor(mTeams.Count));
                mTeams.Add(name, t);

                mStreamWriter.WriteLine("{0}\t{1,20}\t{2}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), id, name);
            }

            NewTeamAdded?.Invoke(t);
            return t;
        }

        private void RestoreTeams() {
            try {
                using (var stream = new StreamReader("Teams/registered.txt")) {
                    string line;

                    while ((line = stream.ReadLine()) != null) {
                        if (line == string.Empty || line[0] == '#') continue;

                        var parts = line.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);

                        long id = long.Parse(parts[1]);
                        string name = parts[2];

                        Team t = new Team(id, name, mEvaluator, ColorHelper.GetPredefiniedColor(mTeams.Count));
                        mTeams.Add(name, t);
                        NewTeamAdded?.Invoke(t);
                    }
                }
            } catch (IOException) { }
        }
    }
}
