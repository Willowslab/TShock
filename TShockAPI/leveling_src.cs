using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TerrariaApi.Server;

namespace TShockAPI
{
    class leveling_src
    {
        public int XPtable(int npctype)
        {
            int xpval = 0;

            if (npctype == 1)
                xpval = 25;

            else if (npctype == 2)
                xpval = 60;

            else if (npctype == 3)
                xpval = 45;

            else if (npctype == 4)
                xpval = 2800;

            else if (npctype == 5)
                xpval = 8;

            else if (npctype == 6)
                xpval = 40;

            else if (npctype == 7)
                xpval = 100;

            else if (npctype == 8)
                xpval = 100;

            else if (npctype == 9)
                xpval = 100;

            else if (npctype == 10)
                xpval = 30;

            else if (npctype == 11)
                xpval = 30;

            else if (npctype == 12)
                xpval = 30;

            else if (npctype == 13)
                xpval = 65;

            else if (npctype == 14)
                xpval = 150;

            else if (npctype == 15)
                xpval = 220;

            else if (npctype == 16)
                xpval = 90;

            else if (npctype == 17)
                xpval = 0; //NPC

            else if (npctype == 18)
                xpval = 0; //NPC

            else if (npctype == 19)
                xpval = 0; //NPC

            else if (npctype == 20)
                xpval = 0; //NPC

            else if (npctype == 21)
                xpval = 60;

            else if (npctype == 22)
                xpval = 0; //NPC

            else if (npctype == 23)
                xpval = 26;

            else if (npctype == 24)
                xpval = 70;

            else if (npctype == 25)
                xpval = 1;

            else if (npctype == 26)
                xpval = 60;

            else if (npctype == 27)
                xpval = 80;

            else if (npctype == 28)
                xpval = 110;

            return xpval;
        }
    }
}
