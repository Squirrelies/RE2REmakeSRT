using System.Collections.Generic;

namespace RE2REmakeSRT
{
    public readonly struct EnemyHP
    {
        public readonly int MaximumHP;
        public readonly int CurrentHP;
        public readonly bool IsAlive;
        public readonly float Percentage;

        public EnemyHP(int maximumHP, int currentHP)
        {
            MaximumHP = maximumHP;
            CurrentHP = currentHP;
            IsAlive = MaximumHP > 0 && CurrentHP > 0;
            Percentage = (IsAlive) ? (float)CurrentHP / (float)MaximumHP : 0f;
        }
    }
}
