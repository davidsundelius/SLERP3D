#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Net;
using RacingGame.Logic;
using RacingGame.Graphics;

#endregion

namespace RacingGame.Network
{
    public class P2PManager : Logic.IUpdateable
    {
        #region Fields

        public const int maxGamers = 4;

        private static readonly P2PManager instance = new P2PManager();
        public static P2PManager Instance { get { return instance; } }

        //public for debug
        public NetworkSession networkSession;

        public bool IsEveryoneReady
        {
            get
            {
                return networkSession.IsEveryoneReady;
            }
        }

        public int CurrentNrOfPlayers
        {
            get
            {
                if (networkSession == null)
                {
                    return 0;
                }
                return networkSession.AllGamers.Count;
            }
        }

        public NetworkGamer LocalGamer
        {
            get
            {
                foreach (var gamer in networkSession.LocalGamers)
                    return gamer;
                return null;
            }
        }


        PacketWriter packetWriter = new PacketWriter();
        PacketReader packetReader = new PacketReader();

        string errorMessage;

        SpriteBatch spriteBatch = new SpriteBatch(GraphicsManager.getDevice());
        SpriteFont font = RacingGame.contentManager.Load<SpriteFont>("Fonts/HUD");

        #endregion


        private P2PManager() { }

        public bool IsActiveSession()
        {
            if (networkSession == null)
            {
                return false;
            }
            return true;
        }

        public void StartGame()
        {
            networkSession.StartGame();
        }

        #region Update

        public bool update(GameTime gameTime)
        {
            // Update our locally controlled tanks, and send their
            // latest position data to everyone in the session.
            foreach (LocalNetworkGamer gamer in networkSession.LocalGamers)
            {
                UpdateLocalGamer(gamer);
            }

            // Pump the underlying session object.
            networkSession.Update();

            // Make sure the session has not ended.
            if (networkSession == null)
                return true; //maybe not

            // Read any packets telling us the positions of remotely controlled tanks.
            foreach (LocalNetworkGamer gamer in networkSession.LocalGamers)
            {
                ReadIncomingPackets(gamer);
            }
            return false;
        }


        /// <summary>
        /// Starts hosting a new network session.
        /// </summary>
        public void CreateSession()
        {
            DrawMessage("Creating session...");

            try
            {
                networkSession = NetworkSession.Create(NetworkSessionType.SystemLink,
                                                       1, maxGamers);
                networkSession.AllowHostMigration = true;
                networkSession.AllowJoinInProgress = false;
                HookSessionEvents();
            }
            catch (Exception e)
            {
                errorMessage = e.Message;
            }
        }


        /// <summary>
        /// Joins an existing network session.
        /// </summary>
        public void JoinSession()
        {
            DrawMessage("Joining session...");
            

            try
            {
                // Search for sessions.
                using (AvailableNetworkSessionCollection availableSessions =
                            NetworkSession.Find(NetworkSessionType.SystemLink,
                                                1, null))
                {
                    if (availableSessions.Count == 0)
                    {
                        errorMessage = "No network sessions found.";
                        return;
                    }

                    // Join the first session we found.
                    networkSession = NetworkSession.Join(availableSessions[0]);

                    HookSessionEvents();
                }
            }
            catch (Exception e)
            {
                errorMessage = e.Message;
            }
        }


        /// <summary>
        /// After creating or joining a network session, we must subscribe to
        /// some events so we will be notified when the session changes state.
        /// </summary>
        void HookSessionEvents()
        {
            networkSession.GamerJoined += GamerJoinedEventHandler;
            networkSession.SessionEnded += SessionEndedEventHandler;
        }


        /// <summary>
        /// This event handler will be called whenever a new gamer joins the session.
        /// </summary>
        void GamerJoinedEventHandler(object sender, GamerJoinedEventArgs e)
        {
            foreach (var gamer in networkSession.AllGamers)
            {
                if (gamer.Tag == null)
                {
                    //in case there are peculiar values in the settings file:
                    ModelNode tag = new ModelNode(RacingGame.contentManager.Load<Model>("Models/Ships/box_01"));
                    switch (Properties.Settings.Default.chosenVehicle)
                    {
                        case 0:
                            tag = new ShipNode(RacingGame.contentManager.Load<Model>("Models/Ships/box_01"));
                            break;
                        case 1:
                            tag = new ShipNode(RacingGame.contentManager.Load<Model>("Models/Ships/ship_02"));
                            break;
                    }
                    gamer.Tag = tag;
                    if (!gamer.IsLocal)
                        States.Game.scene.addNode(e.Gamer.Tag as Node);
                }
            }
        }


        /// <summary>
        /// Event handler notifies us when the network session has ended.
        /// </summary>
        void SessionEndedEventHandler(object sender, NetworkSessionEndedEventArgs e)
        {
            errorMessage = e.EndReason.ToString();

            networkSession.Dispose();
            networkSession = null;
        }


        /// <summary>
        /// Helper for updating a locally controlled gamer.
        /// </summary>
        void UpdateLocalGamer(LocalNetworkGamer gamer)
        {
            // Look up what node is associated with this local player.
            ShipNode localNode = gamer.Tag as ShipNode;

            packetWriter.Write('S');
            // Write the node state into a network packet.
            packetWriter.Write(localNode.position);
            packetWriter.Write(localNode.rotation);
            packetWriter.Write(localNode.useSpeed);

            // Send the data to everyone in the session.
            gamer.SendData(packetWriter, SendDataOptions.InOrder);
        }

        public void SendFinished(float time, float bestLap)
        {
            packetWriter.Write('F');

            packetWriter.Write(time);
            packetWriter.Write(bestLap);
            (LocalGamer as LocalNetworkGamer).SendData(packetWriter, SendDataOptions.InOrder);
        }

        public void SendPlayerData(int ship)
        {
            packetWriter.Write('P');

            packetWriter.Write(ship);
            (LocalGamer as LocalNetworkGamer).SendData(packetWriter, SendDataOptions.InOrder);
        }

        public void SendShield(bool shield)
        {
            packetWriter.Write('s');
            packetWriter.Write(shield);
            (LocalGamer as LocalNetworkGamer).SendData(packetWriter, SendDataOptions.InOrder);
        }

        public void SendPowerpack()
        {
            packetWriter.Write('p');
            (LocalGamer as LocalNetworkGamer).SendData(packetWriter, SendDataOptions.InOrder);
        }

        public void SendDemon(bool demon)
        {
            packetWriter.Write('d');
            packetWriter.Write(demon);
            (LocalGamer as LocalNetworkGamer).SendData(packetWriter, SendDataOptions.InOrder);
        }

        public void SendMissile(Vector3 from, Vector3 explosion)
        {
            packetWriter.Write('m');
            // Write the node state into a network packet.
            packetWriter.Write(from);
            packetWriter.Write(explosion);

            // Send the data to everyone in the session.
            (LocalGamer as LocalNetworkGamer).SendData(packetWriter, SendDataOptions.InOrder);
        }

        public void SendSpeedup(bool speedup)
        {
            packetWriter.Write('u');
            packetWriter.Write(speedup);
            (LocalGamer as LocalNetworkGamer).SendData(packetWriter, SendDataOptions.InOrder);
        }

        public void SendDestroyed()
        {
            packetWriter.Write('C');
            (LocalGamer as LocalNetworkGamer).SendData(packetWriter, SendDataOptions.InOrder);
        }

        public void SendRespawn(Vector3 position)
        {
            packetWriter.Write('R');
            packetWriter.Write(position);
            (LocalGamer as LocalNetworkGamer).SendData(packetWriter, SendDataOptions.InOrder);
        }


        /// <summary>
        /// Helper for reading incoming network packets.
        /// </summary>
        void ReadIncomingPackets(LocalNetworkGamer gamer)
        {
            // Keep reading as long as incoming packets are available.
            while (gamer.IsDataAvailable)
            {
                NetworkGamer sender;

                // Read a single packet from the network.
                gamer.ReceiveData(packetReader, out sender);

                // Discard packets sent by local gamers: we already know their state!
                if (sender.IsLocal)
                    continue;

                // Look up the ship associated with whoever sent this packet.
                ShipNode remoteNode = sender.Tag as ShipNode;

                char type = packetReader.ReadChar();

                switch (type)
                {
                    case 'S':   // update the ship state
                        // Read the state of this ship from the network packet.
                        remoteNode.position = packetReader.ReadVector3();
                        remoteNode.rotation = packetReader.ReadQuaternion();
                        remoteNode.useSpeed = packetReader.ReadBoolean();
                        break;
                    case 'F':   // finished
                        float time = packetReader.ReadSingle();
                        float bestLap = packetReader.ReadSingle();
                        States.Game.Instance.finishedPlayers.Add(new States.FinishedPlayer()
                            { Name = sender.Gamertag, BestLap = bestLap, Time = time });
                        break;
                    case 'P':   // receive player data
                        Model model = null;
                        int shipIndex = packetReader.ReadInt32();
                        switch (shipIndex)
                        {
                            case 0:
                                model = RacingGame.contentManager.Load<Model>("Models/Ships/box_01");
                                break;
                            case 1:
                                model = RacingGame.contentManager.Load<Model>("Models/Ships/ship_02");
                                break;
                        }
                        remoteNode.setModel(model);
                        //SendPlayerData(Properties.Settings.Default.chosenVehicle);  // send to new player
                        break;
                    case 'C':   // ship is destroyed
                        States.Game.spawnExplosion(remoteNode.position);
                        remoteNode.Visible = false;
                        break;
                    case 'R':   // respawn
                        remoteNode.Visible = true;
                        States.Game.spawnRespawnAura(packetReader.ReadVector3());
                        break;
                    case 'd':   // toggle demon mode
                        remoteNode.demonMode = packetReader.ReadBoolean();
                        break;
                    case 's':   // toggle shield
                        remoteNode.shield.Visible = packetReader.ReadBoolean();
                        break;
                    case 'p':   // remote player user powerpack
                        States.Game.spawnGlow(remoteNode.position);
                        break;
                    case 'm':   // missile
                        Vector3 from = packetReader.ReadVector3();
                        Vector3 explosion = packetReader.ReadVector3();
                        Powerups.Missile.spawnTrailParticles(from, explosion);
                        States.Game.spawnExplosion(explosion);
                        break;
                    case 'u':   // toggle speedup
                        remoteNode.useSpeed = packetReader.ReadBoolean();
                        break;
                    default:
                        break;
                }
            }
        }


        #endregion

        #region Drawing

        public void render()
        {
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.BackToFront, SaveStateMode.SaveState);

            // For each person in the session...
            foreach (NetworkGamer gamer in networkSession.RemoteGamers)
            {
                // Look up the tank object belonging to this network gamer.
                Node node = gamer.Tag as Node;

                // Draw a gamertag label.
                string label = gamer.Gamertag;
                if (gamer.IsHost)
                    label += " (host)";
                Color labelColor = Color.Purple;
                float labelScale = 0.6f;
                Vector2 labelSize = font.MeasureString(label);
                Vector2 labelOffset = new Vector2(labelSize.X / 2, 120 + labelSize.Y);

                // Flash the gamertag to yellow when the player is talking.
                if (gamer.IsTalking)
                    labelColor = Color.Yellow;

                Point point = GraphicsHelper.Convert3DPointTo2D(node.position);
                spriteBatch.DrawString(font, label, new Vector2(point.X, point.Y), labelColor, 0,
                    labelOffset, labelScale, SpriteEffects.None, 0);
            }

            spriteBatch.End();
        }


        /// <summary>
        /// Helper draws notification messages before calling blocking network methods.
        /// </summary>
        void DrawMessage(string message)
        {
            //maybe clear graphics device?
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.BackToFront, SaveStateMode.SaveState);

            spriteBatch.DrawString(font, message, new Vector2(161, 161), Color.Black);
            spriteBatch.DrawString(font, message, new Vector2(160, 160), Color.White);

            spriteBatch.End();
        }


        #endregion
    }
}
