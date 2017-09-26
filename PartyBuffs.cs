﻿namespace CurePlease
{
    using System;
    using System.Windows.Forms;
    using static Form1;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;
    using System.Drawing;

    public partial class PartyBuffs : Form
    {
        private Form1 f1;

        public class BuffList
        {
            public string ID { get; set; }
            public string Name { get; set; }
        }

        public List<BuffList> XMLBuffList = new List<BuffList>();

        public PartyBuffs(Form1 f)
        {
            InitializeComponent();

            f1 = f;

            if (f1.setinstance2.Enabled == true)
            {

                // Create the required List


                // Read the Buffs file a generate a List to call.
                foreach (XElement BuffElement in XElement.Load("Resources/Buffs.xml").Elements("o"))
                {
                    XMLBuffList.Add(new BuffList() { ID = BuffElement.Attribute("id").Value, Name = BuffElement.Attribute("en").Value });

                }
            }
            else
            {
                MessageBox.Show("No character was selected as the power leveler, close this window and select one.");
            }

        }

        private void update_effects_Tick(object sender, EventArgs e)
        {
            ailment_list.Text = "";

            if (Form2.config.naSpellsenable)
            {
                // Search through current active party buffs
                foreach (BuffStorage ailment in f1.ActiveBuffs)
                {
                    // First add Character name and a Line Break.
                    ailment_list.AppendText(ailment.CharacterName.ToUpper() + "\n");

                    // Now create a list and loop through each buff and name them
                    List<string> named_buffs = ailment.CharacterBuffs.Split(',').ToList();

                    int i = 1;
                    int count = named_buffs.Count();

                    foreach (string acBuff in named_buffs)
                    {
                        i++;

                        var found_Buff = XMLBuffList.Find(r => r.ID == acBuff);

                        if (found_Buff != null)
                        {
                            if (i == count)
                            {
                                ailment_list.AppendText(found_Buff.Name + " ");
                            }
                            else
                            {
                                ailment_list.AppendText(found_Buff.Name + ", ");
                            }
                        }
                    }


                    ailment_list.AppendText("\n\n");

                }

            }
        }





    }
}