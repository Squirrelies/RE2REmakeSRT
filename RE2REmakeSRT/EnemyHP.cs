using System.Diagnostics;

namespace RE2REmakeSRT
{
    [DebuggerDisplay("{_DebuggerDisplay,nq}")]
    public struct EnemyHP
    {
        /// <summary>
        /// Debugger display message.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public string _DebuggerDisplay
        {
            get
            {
                if (IsAlive)
                    return string.Format("{0} / {1} ({2:P1})", CurrentHP, MaximumHP, Percentage);
                else
                    return "DEAD / DEAD (0%)";
            }
        }

        public int MaximumHP { get; private set; }
        public int CurrentHP { get; private set; }
        public bool IsAlive { get; private set; }
        public float Percentage { get; private set; }

        public EnemyHP(int maximumHP, int currentHP)
        {
            MaximumHP = maximumHP;
            CurrentHP = (currentHP <= maximumHP) ? currentHP : 0;
            IsAlive = MaximumHP > 0 && CurrentHP > 0 && CurrentHP <= MaximumHP;
            Percentage = (IsAlive) ? (float)CurrentHP / (float)MaximumHP : 0f;
        }
    }
}
