#region license

// Copyright (c) 2021, andreakarasho
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
// 1. Redistributions of source code must retain the above copyright
//    notice, this list of conditions and the following disclaimer.
// 2. Redistributions in binary form must reproduce the above copyright
//    notice, this list of conditions and the following disclaimer in the
//    documentation and/or other materials provided with the distribution.
// 3. All advertising materials mentioning features or use of this software
//    must display the following acknowledgement:
//    This product includes software developed by andreakarasho - https://github.com/andreakarasho
// 4. Neither the name of the copyright holder nor the
//    names of its contributors may be used to endorse or promote products
//    derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS ''AS IS'' AND ANY
// EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER BE LIABLE FOR ANY
// DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

#endregion

using System;
using ClassicUO.Configuration;
using ClassicUO.Data;
using ClassicUO.Game.Data;
using ClassicUO.Game.GameObjects;
using ClassicUO.Game.Managers;
using ClassicUO.Game.Scenes;
using ClassicUO.Game.UI.Gumps;
using ClassicUO.Input;
using ClassicUO.Network;
using ClassicUO.Resources;
using ClassicUO.Utility;
using ClassicUO.Utility.Logging;
using Microsoft.Xna.Framework;
using static ClassicUO.Network.NetClient;

namespace ClassicUO.Game
{
    internal static class GameActions
    {
        public static int LastSpellIndex { get; set; } = 1;
        public static int LastSkillIndex { get; set; } = 1;

        public static void ToggleWarMode()
        {
            RequestWarMode(!World.Player.InWarMode);
        }

        public static void RequestWarMode(bool war)
        {
            //Console.WriteLine("RequestWarMode()");
            //Console.WriteLine("war: {0}\n", war);

            if (!World.Player.IsDead)
            {
                if (war && ProfileManager.CurrentProfile != null && ProfileManager.CurrentProfile.EnableMusic)
                {
                    Client.Game.Scene.Audio.PlayMusic((RandomHelper.GetValue(0, 3) % 3) + 38, true);
                }
                else if (!war)
                {
                    Client.Game.Scene.Audio.StopWarMusic();
                }
            }

            Socket.Send_ChangeWarMode(war);
        }

        public static void OpenMacroGump(string name)
        {
            MacroGump macroGump = UIManager.GetGump<MacroGump>();

            macroGump?.Dispose();
            UIManager.Add(new MacroGump(name));
        }

        public static void OpenPaperdoll(uint serial)
        {
            //Console.WriteLine("OpenPaperdoll()");

            PaperDollGump paperDollGump = UIManager.GetGump<PaperDollGump>(serial);
            if (paperDollGump == null)
            {
                DoubleClick(serial | 0x80000000);
            }
            else
            {
                if (paperDollGump.IsMinimized)
                {
                    paperDollGump.IsMinimized = false;
                }

                paperDollGump.SetInScreen();
                paperDollGump.BringOnTop();
            }
        }

        public static void OpenSettings(int page = 0)
        {
            OptionsGump opt = UIManager.GetGump<OptionsGump>();
            if (opt == null)
            {
                OptionsGump optionsGump = new OptionsGump
                {
                    X = (Client.Game.Window.ClientBounds.Width >> 1) - 300,
                    Y = (Client.Game.Window.ClientBounds.Height >> 1) - 250
                };

                UIManager.Add(optionsGump);
                optionsGump.ChangePage(page);
                optionsGump.SetInScreen();
            }
            else
            {
                opt.SetInScreen();
                opt.BringOnTop();
            }
        }

        public static void OpenStatusBar()
        {
            Client.Game.Scene.Audio.StopWarMusic();
            if (StatusGumpBase.GetStatusGump() == null)
            {
                UIManager.Add(StatusGumpBase.AddStatusGump(100, 100));
            }
        }

        public static void OpenJournal()
        {
            JournalGump journalGump = UIManager.GetGump<JournalGump>();
            if (journalGump == null)
            {
                UIManager.Add(new JournalGump { X = 64, Y = 64 });
            }
            else
            {
                journalGump.SetInScreen();
                journalGump.BringOnTop();

                if (journalGump.IsMinimized)
                {
                    journalGump.IsMinimized = false;
                }
            }
        }

        public static void OpenSkills()
        {
            StandardSkillsGump skillsGump = UIManager.GetGump<StandardSkillsGump>();
            if (skillsGump != null && skillsGump.IsMinimized)
            {
                skillsGump.IsMinimized = false;
            }
            else
            {
                World.SkillsRequested = true;
                Socket.Send_SkillsRequest(World.Player.Serial);
            }
        }

        public static void OpenMiniMap()
        {
            MiniMapGump miniMapGump = UIManager.GetGump<MiniMapGump>();
            if (miniMapGump == null)
            {
                UIManager.Add(new MiniMapGump());
            }
            else
            {
                miniMapGump.ToggleSize();
                miniMapGump.SetInScreen();
                miniMapGump.BringOnTop();
            }
        }

        public static void OpenWorldMap()
        {
            WorldMapGump worldMap = UIManager.GetGump<WorldMapGump>();
            if (worldMap == null || worldMap.IsDisposed)
            {
                worldMap = new WorldMapGump();
                UIManager.Add(worldMap);
            }
            else
            {
                worldMap.BringOnTop();
                worldMap.SetInScreen();
            }
        }

        public static void OpenChat()
        {
            if (ChatManager.ChatIsEnabled == ChatStatus.Enabled)
            {
                ChatGump chatGump = UIManager.GetGump<ChatGump>();

                if (chatGump == null)
                {
                    UIManager.Add(new ChatGump());
                }
                else
                {
                    chatGump.SetInScreen();
                    chatGump.BringOnTop();
                }
            }
            else if (ChatManager.ChatIsEnabled == ChatStatus.EnabledUserRequest)
            {
                ChatGumpChooseName chatGump = UIManager.GetGump<ChatGumpChooseName>();

                if (chatGump == null)
                {
                    UIManager.Add(new ChatGumpChooseName());
                }
                else
                {
                    chatGump.SetInScreen();
                    chatGump.BringOnTop();
                }
            }
        }

        public static bool OpenCorpse(uint serial)
        {
            //Console.WriteLine("OpenCorpse()");

            if (!SerialHelper.IsItem(serial))
            {
                return false;
            }

            Item item = World.Items.Get(serial);
            if (item == null || !item.IsCorpse || item.IsDestroyed)
            {
                return false;
            }

            World.Player.ManualOpenedCorpses.Add(serial);

            DoubleClick(serial);

            return true;
        }

        public static bool OpenBackpack()
        {
            Console.WriteLine("OpenBackpack()");

            Item backpack = World.Player.FindItemByLayer(Layer.Backpack);
            //Console.WriteLine("backpack / serial: {0}\n", backpack.Serial);

            if (backpack == null)
            {
                return false;
            }

            ContainerGump backpackGump = UIManager.GetGump<ContainerGump>(backpack);
            if (backpackGump == null)
            {
                GameActions.DoubleClick(backpack);
            }
            else
            {
                if (backpackGump.IsMinimized)
                {
                    backpackGump.IsMinimized = false;
                }

                backpackGump.SetInScreen();
                backpackGump.BringOnTop();
            }

            return true;
        }

        public static void Attack(uint serial)
        {
            //Console.WriteLine("GameActions Attack()");

            if (ProfileManager.CurrentProfile.EnabledCriminalActionQuery)
            {
                Mobile m = World.Mobiles.Get(serial);
                if (m != null && (World.Player.NotorietyFlag == NotorietyFlag.Innocent || World.Player.NotorietyFlag == NotorietyFlag.Ally) && m.NotorietyFlag == NotorietyFlag.Innocent && m != World.Player)
                {
                    QuestionGump messageBox = new QuestionGump
                    (
                        ResGeneral.ThisMayFlagYouCriminal,
                        s =>
                        {
                            if (s)
                            {
                                Socket.Send_AttackRequest(serial);
                            }
                        }
                    );

                    UIManager.Add(messageBox);

                    return;
                }
            }

            TargetManager.LastAttack = serial;

            Entity ent = World.Get(serial);
            //Console.WriteLine("ent.Name: {0}", ent.Name);

            Socket.Send_AttackRequest(serial);
        }

        public static void DoubleClickQueued(uint serial)
        {
            Client.Game.GetScene<GameScene>()?.DoubleClickDelayed(serial);
        }

        public static void DoubleClick(uint serial)
        {
            //Log.Trace("DoubleClick");
            //Console.WriteLine("DoubleClick()");

            Client.Game._uoServiceImpl.SetActionType(2);
            Client.Game._uoServiceImpl.SetTargetSerial(serial);
            
            if (serial != World.Player && SerialHelper.IsMobile(serial) && World.Player.InWarMode)
            {
                RequestMobileStatus(serial);
                Attack(serial);
            }
            else
            {
                //Console.WriteLine("Socket.Send_DoubleClick(serial)");
                Socket.Send_DoubleClick(serial);
            }

            if (SerialHelper.IsItem(serial))
            {
                World.OPL.TryGetNameAndData(serial, out string name, out string data);
                //Console.WriteLine("DoubleClick(), SerialHelper.IsItem({0}), name: {1}", serial, name);

                World.LastObject = serial;
            }
            else
            {
                World.LastObject = 0;
            }
        }

        public static void SingleClick(uint serial)
        {
            // add request context menu
            Socket.Send_ClickRequest(serial);

            Entity entity = World.Get(serial);

            if (entity != null)
            {
                entity.IsClicked = true;
            }

            World.OPL.TryGetNameAndData(serial, out string name, out string data);
            //Console.WriteLine("SingleClick(), SerialHelper.IsItem({0}), name: {1}", serial, name);
        }

        public static void Say(string message, ushort hue = 0xFFFF, MessageType type = MessageType.Regular, byte font = 3)
        {
            if (hue == 0xFFFF)
            {
                hue = ProfileManager.CurrentProfile.SpeechHue;
            }

            // TODO: identify what means 'older client' that uses ASCIISpeechRquest [0x03]
            // 
            // Fix -> #1267
            if (Client.Version >= ClientVersion.CV_200)
            {
                Socket.Send_UnicodeSpeechRequest(message,
                                                 type,
                                                 font,
                                                 hue,
                                                 Settings.GlobalSettings.Language);
            }
            else
            {
                Socket.Send_ASCIISpeechRequest(message, type, font, hue);
            }
        }

        public static void Print(string message, ushort hue = 946, MessageType type = MessageType.Regular, byte font = 3, bool unicode = true)
        {
            Print
            (
                null,
                message,
                hue,
                type,
                font,
                unicode
            );
        }

        public static void Print
        (
            Entity entity,
            string message,
            ushort hue = 946,
            MessageType type = MessageType.Regular,
            byte font = 3,
            bool unicode = true
        )
        {
            //Console.WriteLine("GameActions Print()");

            MessageManager.HandleMessage
            (
                entity,
                message,
                entity != null ? entity.Name : "System",
                hue,
                type,
                font,
                entity == null ? TextType.SYSTEM : TextType.OBJECT,
                unicode,
                Settings.GlobalSettings.Language
            );
        }

        public static void SayParty(string message, uint serial = 0)
        {
            Socket.Send_PartyMessage(message, serial);
        }

        public static void RequestPartyAccept(uint serial)
        {
            Socket.Send_PartyAccept(serial);

            UIManager.GetGump<PartyInviteGump>()?.Dispose();
        }

        public static void RequestPartyRemoveMemberByTarget()
        {
            Socket.Send_PartyRemoveRequest(0x00);
        }

        public static void RequestPartyRemoveMember(uint serial)
        {
            Socket.Send_PartyRemoveRequest(serial);
        }

        public static void RequestPartyQuit()
        {
            Socket.Send_PartyRemoveRequest(World.Player.Serial);
        }

        public static void RequestPartyInviteByTarget()
        {
            Socket.Send_PartyInviteRequest();
        }

        public static void RequestPartyLootState(bool isLootable)
        {
            Socket.Send_PartyChangeLootTypeRequest(isLootable);
        }

        public static bool PickUp
        (
            uint serial,
            int x,
            int y,
            int amount = -1,
            Point? offset = null,
            bool is_gump = false
        )
        {
            //Console.WriteLine("PickUp()");

            if (World.Player.IsDead || ItemHold.Enabled)
            {
                return false;
            }

            Item item = World.Items.Get(serial);
            if (item == null || item.IsDestroyed || item.IsMulti || item.OnGround && (item.IsLocked || item.Distance > Constants.DRAG_ITEMS_DISTANCE))
            {
                return false;
            }

            if (amount <= -1 && item.Amount > 1 && item.ItemData.IsStackable)
            {
                if (ProfileManager.CurrentProfile.HoldShiftToSplitStack == Keyboard.Shift)
                {
                    SplitMenuGump gump = UIManager.GetGump<SplitMenuGump>(item);
                    if (gump != null)
                    {
                        return false;
                    }

                    gump = new SplitMenuGump(item, new Point(x, y))
                    {
                        X = Mouse.Position.X - 80,
                        Y = Mouse.Position.Y - 40
                    };

                    UIManager.Add(gump);
                    UIManager.AttemptDragControl(gump, true);

                    return true;
                }
            }

            if (amount <= 0)
            {
                amount = item.Amount;
            }

            Client.Game._uoServiceImpl.SetActionType(3);
            Client.Game._uoServiceImpl.SetTargetSerial(serial);
            Client.Game._uoServiceImpl.SetAmount((uint) amount);

            ItemHold.Clear();
            ItemHold.Set(item, (ushort) amount, offset);
            ItemHold.IsGumpTexture = is_gump;
            Socket.Send_PickUpRequest(item, (ushort) amount);

            if (item.OnGround)
            {
                item.RemoveFromTile();
            }

            item.TextContainer?.Clear();

            World.ObjectToRemove = item.Serial;

            Client.Game._uoServiceImpl.SetUpdatePlayerObjectTimer(2);

            return true;
        }

        public static void DropItem(uint serial, int x, int y, int z, uint container)
        {
            //Console.WriteLine("DropItem()");

            Client.Game._uoServiceImpl.SetActionType(4);
            Client.Game._uoServiceImpl.SetSourceSerial(serial);

            World.OPL.TryGetNameAndData(container, out string name, out string data);
            //Console.WriteLine("x: {0}, y: {1}, z: {2}, container: {3}, name: {4}", x, y, z, container, name);
            
            if (container == 0xFFFF_FFFF)
            {
                // Drop on the land
                Client.Game._uoServiceImpl.SetUsedLand(x, y);
            }
            else
            {
                // Drop on the item, mobile
                if ( (x == 0xFFFF) && (y == 0xFFFF) )
                {
                    // 1. Drop on the mobile
                    // x: 65535, y: 65535, container name:  Mugg The Miner

                    // 1. Drop on the container default position
                    // x: 65535, y: 65535, container name: Backpack
                    //Console.WriteLine("1. Drop on the mobile or the container default position");
                    //Console.WriteLine("container name: {0}", name);
                    Client.Game._uoServiceImpl.SetTargetSerial(container);
                    Client.Game._uoServiceImpl.SetIndex(0);
                }
                else
                {
                    BaseGameObject obj = SelectedObject.Object;

                    Item SelectedItem = null;
                    if (obj is Item)
                    {
                        SelectedItem = (Item) obj;
                    }
                    else
                    {
                        SelectedItem = null;
                    }

                    if (SelectedItem == null)
                    {
                        // 2. Drop on the container fixed position.
                        //Console.WriteLine("2. Drop on the container fixed position.");
                        Client.Game._uoServiceImpl.SetTargetSerial(container);
                        Client.Game._uoServiceImpl.SetIndex(1);
                    }
                    else
                    {
                        //Console.WriteLine("container: {0}", container);
                        Item containerItem = World.Items.Get(container);

                        World.OPL.TryGetNameAndData(containerItem.Container, out string containerName, 
                                                    out string containerEata);

                        if (SelectedItem.Serial == container)
                        {
                            // 1. Drop on the same type item. This is actually same as case 1.
                            //Console.WriteLine("1. Drop on the same type item. This is actually same as case 1");
                            //Console.WriteLine("container name: {0}", name);
                            Client.Game._uoServiceImpl.SetTargetSerial(container);
                            Client.Game._uoServiceImpl.SetIndex(0);
                        }
                        else
                        {
                            // 2. Drop on the different type item.
                            // This is actually same as drop on the container default position.
                            //Console.WriteLine("2. Drop on the different type item. This is actually same as dropping on the container default position.");
                            //Console.WriteLine("container name: {0}", name);
                            Client.Game._uoServiceImpl.SetTargetSerial(container);
                            Client.Game._uoServiceImpl.SetIndex(1);
                        }
                    }
                }
            }

            Item bank = World.Player.FindItemByLayer(Layer.Bank);
            if (container == bank.Serial)
            {
                if (World.Player.BankOpened == false)
                {
                    Console.WriteLine("BankOpened is not opened");
                    //ItemHold.Enabled = false;
                    //ItemHold.Dropped = true;
                    //ItemHold.Clear();
                    return ;
                }
            }

            if (ItemHold.Enabled && !ItemHold.IsFixedPosition && (ItemHold.Serial != container || ItemHold.ItemData.IsStackable))
            {
                if (Client.Version >= ClientVersion.CV_6017)
                {
                    Socket.Send_DropRequest(serial,
                                            (ushort)x,
                                            (ushort)y,
                                            (sbyte)z,
                                            0,
                                            container);
                }
                else
                {
                    Socket.Send_DropRequest_Old(serial,
                                                (ushort)x,
                                                (ushort)y,
                                                (sbyte)z,
                                                container);
                }

                ItemHold.Enabled = false;
                ItemHold.Dropped = true;
                ItemHold.Clear();
            }

            Client.Game._uoServiceImpl.SetUpdatePlayerObjectTimer(2);
        }

        public static void Equip(uint container = 0)
        {
            //Console.WriteLine("Equip(), container: {0}", container);

            if (ItemHold.Enabled && !ItemHold.IsFixedPosition && ItemHold.ItemData.IsWearable)
            {
                if (!SerialHelper.IsValid(container))
                {
                    container = World.Player.Serial;
                }

                Socket.Send_EquipRequest(ItemHold.Serial, (Layer)ItemHold.ItemData.Layer, container);

                ItemHold.Enabled = false;
                ItemHold.Dropped = true;
                ItemHold.Clear();
            }
        }

        public static void ReplyGump(uint local, uint server, int button, uint[] switches = null, Tuple<ushort, string>[] entries = null)
        {
            //Console.WriteLine("ReplyGump()");
            //Console.WriteLine("local: {0}, server: {1}, button: {2}, switches: {3}, entries: {4}", 
            //                   local, server, button, switches, entries );

            for (int i = 0; i < entries.Length; i++)
            {
                //Console.WriteLine("entries[{0}]: {1}", i, entries[i]);
            } 

            Console.WriteLine("switches: {0}, entries: {1}", switches, entries);

            Socket.Send_GumpResponse(local, server, button, switches, entries);

            //Console.WriteLine("");
        }

        public static void RequestHelp()
        {
            Socket.Send_HelpRequest();
        }

        public static void RequestQuestMenu()
        {
            Socket.Send_QuestMenuRequest();
        }

        public static void RequestProfile(uint serial)
        {
            Socket.Send_ProfileRequest(serial);
        }

        public static void ChangeSkillLockStatus(ushort skillindex, byte lockstate)
        {
            //Console.WriteLine("ChangeSkillLockStatus(), skillindex: {0}, lockstate: {1}", skillindex, lockstate);
            Socket.Send_SkillStatusChangeRequest(skillindex, lockstate);
        }

        public static void RequestMobileStatus(uint serial, bool force = false)
        {
            if (World.InGame)
            {
                Entity ent = World.Get(serial);
                if (ent != null)
                {
                    if (force)
                    {
                        if (ent.HitsRequest >= HitsRequestStatus.Pending)
                        {
                            SendCloseStatus(serial);
                        }
                    }

                    if (ent.HitsRequest < HitsRequestStatus.Received)
                    {
                        ent.HitsRequest = HitsRequestStatus.Pending;
                        force = true;
                    }
                }

                if (force && SerialHelper.IsValid(serial))
                {
                    //ent = ent ?? World.Player;
                    //ent.AddMessage(MessageType.Regular, $"PACKET SENT: 0x{serial:X8}", 3, 0x34, true, TextType.OBJECT);
                    Socket.Send_StatusRequest(serial);
                }
            }
        }

        public static void SendCloseStatus(uint serial, bool force = false)
        {
            if (Client.Version >= ClientVersion.CV_200 && World.InGame)
            {
                Entity ent = World.Get(serial);

                if (ent != null && ent.HitsRequest >= HitsRequestStatus.Pending)
                {
                    ent.HitsRequest = HitsRequestStatus.None;
                    force = true;
                }

                if (force && SerialHelper.IsValid(serial))
                {
                    //ent = ent ?? World.Player;
                    //ent.AddMessage(MessageType.Regular, $"PACKET REMOVED SENT: 0x{serial:X8}", 3, 0x34 + 10, true, TextType.OBJECT);
                    Socket.Send_CloseStatusBarGump(serial);
                }
            }
        }

        public static void CastSpellFromBook(int index, uint bookSerial)
        {
            if (index >= 0)
            {
                LastSpellIndex = index;
                Socket.Send_CastSpellFromBook(index, bookSerial);
            }
        }

        public static void CastSpell(int index)
        {
            if (index >= 0)
            {
                LastSpellIndex = index;
                Socket.Send_CastSpell(index);
            }
        }

        public static void OpenGuildGump()
        {
            Socket.Send_GuildMenuRequest();
        }

        public static void ChangeStatLock(byte stat, Lock state)
        {
            Socket.Send_StatLockStateRequest(stat, state);
        }

        public static void Rename(uint serial, string name)
        {
            Socket.Send_RenameRequest(serial, name);
        }

        public static void UseSkill(int index)
        {
            //Console.WriteLine("UseSkill()");

            if (index >= 0)
            {
                LastSkillIndex = index;
                Socket.Send_UseSkill(index);
            }
        }

        public static void OpenPopupMenu(uint serial, bool shift = false)
        {
            shift = shift || Keyboard.Shift;

            if (ProfileManager.CurrentProfile.HoldShiftForContext && !shift)
            {
                return;
            }

            Client.Game._uoServiceImpl.SetActionType(10);
            Client.Game._uoServiceImpl.SetTargetSerial(serial);

            Socket.Send_RequestPopupMenu(serial);
        }

        public static void ResponsePopupMenu(uint serial, ushort index)
        {
            //Console.WriteLine("ResponsePopupMenu()");
            //Console.WriteLine("index: {0}", index);

            Client.Game._uoServiceImpl.SetActionType(11);
            Client.Game._uoServiceImpl.SetIndex(index);

            Socket.Send_PopupMenuSelection(serial, index);
        }

        public static void MessageOverhead(string message, uint entity)
        {
            Print(World.Get(entity), message);
        }

        public static void MessageOverhead(string message, ushort hue, uint entity)
        {
            Print(World.Get(entity), message, hue);
        }

        public static void AcceptTrade(uint serial, bool accepted)
        {
            Socket.Send_TradeResponse(serial, 2, accepted);
        }

        public static void CancelTrade(uint serial)
        {
            Socket.Send_TradeResponse(serial, 1, false);
        }

        public static void AllNames()
        {
            foreach (Mobile mobile in World.Mobiles.Values)
            {
                if (mobile != World.Player)
                {
                    Socket.Send_ClickRequest(mobile.Serial);
                }
            }

            foreach (Item item in World.Items.Values)
            {
                if (item.IsCorpse)
                {
                    Socket.Send_ClickRequest(item.Serial);
                }
            }
        }

        public static void OpenDoor()
        {
            Socket.Send_OpenDoor();
        }

        public static void EmoteAction(string action)
        {
            Socket.Send_EmoteAction(action);
        }

        public static void OpenAbilitiesBook()
        {
            if (UIManager.GetGump<CombatBookGump>() == null)
            {
                UIManager.Add(new CombatBookGump(100, 100));
            }
        }

        public static void UsePrimaryAbility()
        {
            //Console.WriteLine("UsePrimaryAbility()");

            ref Ability ability = ref World.Player.Abilities[0];
            if (((byte) ability & 0x80) == 0)
            {
                for (int i = 0; i < 2; i++)
                {
                    World.Player.Abilities[i] &= (Ability) 0x7F;
                }

                Socket.Send_UseCombatAbility((byte)ability);
            }
            else
            {
                Socket.Send_UseCombatAbility(0);
            }

            ability ^= (Ability) 0x80;
        }

        public static void UseSecondaryAbility()
        {
            //Console.WriteLine("UseSecondaryAbility()");

            ref Ability ability = ref World.Player.Abilities[1];

            if (((byte) ability & 0x80) == 0)
            {
                for (int i = 0; i < 2; i++)
                {
                    World.Player.Abilities[i] &= (Ability) 0x7F;
                }

                Socket.Send_UseCombatAbility((byte)ability);
            }
            else
            {
                Socket.Send_UseCombatAbility(0);
            }

            ability ^= (Ability) 0x80;
        }

        public static void QuestArrow(bool rightClick)
        {
            Socket.Send_ClickQuestArrow(rightClick);
        }

        public static void GrabItem(uint serial, ushort amount, uint bag = 0)
        {
            //Console.WriteLine("GrabItem()");
            //Socket.Send(new PPickUpRequest(serial, amount));

            Item backpack = World.Player.FindItemByLayer(Layer.Backpack);

            if (backpack == null)
            {
                return;
            }

            if (bag == 0)
            {
                bag = ProfileManager.CurrentProfile.GrabBagSerial == 0 ? backpack.Serial : ProfileManager.CurrentProfile.GrabBagSerial;
            }

            if (!World.Items.Contains(bag))
            {
                Print(ResGeneral.GrabBagNotFound);
                ProfileManager.CurrentProfile.GrabBagSerial = 0;
                bag = backpack.Serial;
            }

            PickUp(serial, 0, 0, amount);

            DropItem
            (
                serial,
                0xFFFF,
                0xFFFF,
                0,
                bag
            );
        }
    }
}