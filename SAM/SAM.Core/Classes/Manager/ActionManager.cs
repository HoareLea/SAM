using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SAM.Core
{
    public class ActionManager
    {
        List<Action> actions = new List<Action>();

        public event EventHandler RunStarted;
        public event EventHandler RunEnded;

        public ActionManager()
        {

        }

        public ActionManager(IEnumerable<Action> actions)
        {
            this.actions = actions == null ? null : new List<Action>(actions);
        }

        public void Add(Action action)
        {
            if (action == null)
            {
                return;
            }

            if(actions == null)
            {
                actions = new List<Action>();
            }

            actions.Add(action);
        }

        public void AddRange(IEnumerable<Action> actions)
        {
            if (actions == null)
            {
                return;
            }

            if(this.actions == null)
            {
                this.actions = new List<Action>();
            }

            this.actions.AddRange(actions);
        }

        public void Clear()
        {
            actions?.Clear();
        }

        public void Run()
        {
            RunStarted?.Invoke(this, new EventArgs());
            if (actions == null || actions.Count == 0)
            {
                RunEnded?.Invoke(this, new EventArgs());
                return;
            }

            Parallel.For(0, actions.Count, (int i) =>
            {
                actions[i]?.Invoke();
            });

            RunEnded?.Invoke(this, new EventArgs());
        }

    }
}
