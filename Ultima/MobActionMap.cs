/***************************************************************************
 *
 * $Author: Turley
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System.Collections.Generic;

namespace Ultima
{
    /// <summary>
    /// What an action shows, independent of the body type's action layout.
    /// </summary>
    public enum ActionRole
    {
        None,
        Walk,
        Run,
        Idle,
        Fidget,
        Alert,
        Eat,
        LieDown,
        CombatIdle,
        CombatAdvance,
        Attack1,
        Attack2,
        Attack3,
        AttackBow,
        AttackCrossbow,
        AttackThrow,
        Punch,
        Pillage,
        Stomp,
        Cast1,
        Cast2,
        Block,
        GetHit,
        GetHitInAir,
        Die1,
        Die2,
        Fly,
        TakeOff,
        MountWalk,
        MountRun,
        MountIdle,
        MountAttack,
        Emote
    }

    /// <summary>
    /// Maps actions between the monster (22), sea monster (9), animal (13) and human (35) action layouts by
    /// what each action shows, falling back to the closest action the other layout has (run to walk, a third
    /// attack to the first, ...). Equipment uses the human layout.
    /// </summary>
    public static class MobActionMap
    {
        private static readonly ActionRole[] _monster =
        {
            ActionRole.Walk, ActionRole.Idle, ActionRole.Die1, ActionRole.Die2, ActionRole.Attack1, ActionRole.Attack2,
            ActionRole.Attack3, ActionRole.AttackBow, ActionRole.AttackCrossbow, ActionRole.AttackThrow, ActionRole.GetHit,
            ActionRole.Pillage, ActionRole.Stomp, ActionRole.Cast1, ActionRole.Cast2, ActionRole.Block, ActionRole.Block,
            ActionRole.Idle, ActionRole.Fidget, ActionRole.Fly, ActionRole.TakeOff, ActionRole.GetHitInAir
        };

        private static readonly ActionRole[] _sea =
        {
            ActionRole.Walk, ActionRole.Run, ActionRole.Idle, ActionRole.Idle, ActionRole.Fidget, ActionRole.Attack1,
            ActionRole.Attack2, ActionRole.GetHit, ActionRole.Die1
        };

        private static readonly ActionRole[] _animal =
        {
            ActionRole.Walk, ActionRole.Run, ActionRole.Idle, ActionRole.Eat, ActionRole.Alert, ActionRole.Attack1,
            ActionRole.Attack2, ActionRole.GetHit, ActionRole.Die1, ActionRole.Idle, ActionRole.Fidget, ActionRole.LieDown,
            ActionRole.Die2
        };

        private static readonly ActionRole[] _human =
        {
            ActionRole.Walk, ActionRole.Walk, ActionRole.Run, ActionRole.Run, ActionRole.Idle, ActionRole.Idle,
            ActionRole.Fidget, ActionRole.CombatIdle, ActionRole.CombatIdle, ActionRole.Attack1, ActionRole.Attack2,
            ActionRole.Attack3, ActionRole.Attack3, ActionRole.Attack1, ActionRole.Attack2, ActionRole.CombatAdvance,
            ActionRole.Cast1, ActionRole.Cast2, ActionRole.AttackBow, ActionRole.AttackCrossbow, ActionRole.GetHit,
            ActionRole.Die1, ActionRole.Die2, ActionRole.MountWalk, ActionRole.MountRun, ActionRole.MountIdle,
            ActionRole.MountAttack, ActionRole.MountAttack, ActionRole.MountAttack, ActionRole.MountAttack, ActionRole.Block,
            ActionRole.Punch, ActionRole.Emote, ActionRole.Emote, ActionRole.Eat
        };

        // Roles tried, in order, when a layout has no action with the role itself. A mount action falls back
        // to what the mount does under the rider, so a rider's actions map onto the mount's.
        private static readonly Dictionary<ActionRole, ActionRole[]> _fallbacks = new()
        {
            [ActionRole.Run] = new[] { ActionRole.Walk },
            [ActionRole.Fidget] = new[] { ActionRole.Idle },
            [ActionRole.Alert] = new[] { ActionRole.Fidget, ActionRole.Idle },
            [ActionRole.Eat] = new[] { ActionRole.Fidget, ActionRole.Idle },
            [ActionRole.LieDown] = new[] { ActionRole.Idle },
            [ActionRole.CombatIdle] = new[] { ActionRole.Idle },
            [ActionRole.CombatAdvance] = new[] { ActionRole.Walk },
            [ActionRole.Attack2] = new[] { ActionRole.Attack1 },
            [ActionRole.Attack3] = new[] { ActionRole.Attack2, ActionRole.Attack1 },
            [ActionRole.AttackBow] = new[] { ActionRole.AttackThrow, ActionRole.Attack1 },
            [ActionRole.AttackCrossbow] = new[] { ActionRole.AttackBow, ActionRole.AttackThrow, ActionRole.Attack1 },
            [ActionRole.AttackThrow] = new[] { ActionRole.AttackBow, ActionRole.Attack1 },
            [ActionRole.Punch] = new[] { ActionRole.Attack1 },
            [ActionRole.Pillage] = new[] { ActionRole.Attack1 },
            [ActionRole.Stomp] = new[] { ActionRole.Attack2, ActionRole.Attack1 },
            [ActionRole.Cast1] = new[] { ActionRole.Cast2, ActionRole.Attack1 },
            [ActionRole.Cast2] = new[] { ActionRole.Cast1, ActionRole.Attack1 },
            [ActionRole.Block] = new[] { ActionRole.GetHit },
            [ActionRole.GetHitInAir] = new[] { ActionRole.GetHit },
            [ActionRole.Die2] = new[] { ActionRole.Die1 },
            [ActionRole.Fly] = new[] { ActionRole.Walk },
            [ActionRole.TakeOff] = new[] { ActionRole.Fly, ActionRole.Walk },
            [ActionRole.MountWalk] = new[] { ActionRole.Walk },
            [ActionRole.MountRun] = new[] { ActionRole.Run, ActionRole.Walk },
            [ActionRole.MountIdle] = new[] { ActionRole.Idle },
            [ActionRole.MountAttack] = new[] { ActionRole.MountIdle, ActionRole.Idle },
            [ActionRole.Emote] = new[] { ActionRole.Fidget, ActionRole.Idle },
        };

        /// <summary>
        /// The action roles of a layout, indexed by action.
        /// </summary>
        public static IReadOnlyList<ActionRole> GetRoles(MobType type)
        {
            return type switch
            {
                MobType.Monster => _monster,
                MobType.Sea => _sea,
                MobType.Animal => _animal,
                _ => _human
            };
        }

        public static ActionRole GetRole(MobType type, int action)
        {
            IReadOnlyList<ActionRole> roles = GetRoles(type);
            return action >= 0 && action < roles.Count ? roles[action] : ActionRole.None;
        }

        /// <summary>
        /// The action of layout <paramref name="to"/> that best matches <paramref name="action"/> of layout
        /// <paramref name="from"/>, or -1 when the target layout has nothing close. Within one layout an action
        /// maps to itself.
        /// </summary>
        public static int MapAction(MobType from, int action, MobType to)
        {
            from = Normalize(from);
            to = Normalize(to);
            if (from == to)
            {
                return action >= 0 && action < GetRoles(to).Count ? action : -1;
            }

            return FindAction(to, GetRole(from, action));
        }

        /// <summary>
        /// The first action of layout <paramref name="type"/> with <paramref name="role"/> or, failing that, with
        /// the nearest fallback role; -1 when there is none.
        /// </summary>
        public static int FindAction(MobType type, ActionRole role)
        {
            if (role == ActionRole.None)
            {
                return -1;
            }

            IReadOnlyList<ActionRole> roles = GetRoles(Normalize(type));
            int found = IndexOf(roles, role);
            if (found >= 0 || !_fallbacks.TryGetValue(role, out ActionRole[] fallbacks))
            {
                return found;
            }

            foreach (ActionRole fallback in fallbacks)
            {
                found = IndexOf(roles, fallback);
                if (found >= 0)
                {
                    return found;
                }
            }

            return -1;
        }

        private static MobType Normalize(MobType type)
        {
            return type == MobType.Equipment ? MobType.Human : type;
        }

        private static int IndexOf(IReadOnlyList<ActionRole> roles, ActionRole role)
        {
            for (int i = 0; i < roles.Count; ++i)
            {
                if (roles[i] == role)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
