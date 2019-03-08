// Node.java
//   Java Spatial Index Library
//   Copyright (C) 2002 Infomatiq Limited
//
//  This library is free software; you can redistribute it and/or
//  modify it under the terms of the GNU Lesser General Public
//  License as published by the Free Software Foundation; either
//  version 2.1 of the License, or (at your option) any later version.
//
//  This library is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//  Lesser General Public License for more details.
//
//  You should have received a copy of the GNU Lesser General Public
//  License along with this library; if not, write to the Free Software
//  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
// Ported to C# By Dror Gluska, April 9th, 2009

namespace AcadLib.RTree.SpatialIndex
{
    using JetBrains.Annotations;

    // import com.infomatiq.jsi.Rectangle;

    /**
     * <p>Used by RTree. There are no public methods in this class.</p>
     *
     * @author aled@sourceforge.net
     * @version 1.0b2p1
     */
    [PublicAPI]
    public class Node<T>
    {
        internal readonly Rectangle[] entries;
        internal readonly int[] ids;
        internal readonly int level;
        internal readonly int nodeId;
        internal int entryCount;
        internal Rectangle mbr;

        public Node(int nodeId, int level, int maxNodeEntries)
        {
            this.nodeId = nodeId;
            this.level = level;
            entries = new Rectangle[maxNodeEntries];
            ids = new int[maxNodeEntries];
        }

        public Rectangle getEntry(int index)
        {
            if (index < entryCount)
            {
                return entries[index];
            }

            return null;
        }

        public int getEntryCount()
        {
            return entryCount;
        }

        public int getId(int index)
        {
            if (index < entryCount)
            {
                return ids[index];
            }

            return -1;
        }

        public int getLevel()
        {
            return level;
        }

        public Rectangle getMBR()
        {
            return mbr;
        }

        internal void addEntry([NotNull] Rectangle r, int id)
        {
            ids[entryCount] = id;
            entries[entryCount] = r.Copy();
            entryCount++;
            if (mbr == null)
            {
                mbr = r.Copy();
            }
            else
            {
                mbr.Add(r);
            }
        }

        internal void addEntryNoCopy(Rectangle r, int id)
        {
            ids[entryCount] = id;
            entries[entryCount] = r;
            entryCount++;
            if (mbr == null)
            {
                mbr = r.Copy();
            }
            else
            {
                mbr.Add(r);
            }
        }

        // delete entry. This is done by setting it to null and copying the last entry into its space.
        internal void deleteEntry(int i, int minNodeEntries)
        {
            var lastIndex = entryCount - 1;
            var deletedRectangle = entries[i];
            entries[i] = null;
            if (i != lastIndex)
            {
                entries[i] = entries[lastIndex];
                ids[i] = ids[lastIndex];
                entries[lastIndex] = null;
            }

            entryCount--;

            // if there are at least minNodeEntries, adjust the MBR.
            // otherwise, don't bother, as the Node<T> will be
            // eliminated anyway.
            if (entryCount >= minNodeEntries)
            {
                recalculateMBR(deletedRectangle);
            }
        }

        // Return the index of the found entry, or -1 if not found
        internal int findEntry(Rectangle r, int id)
        {
            for (var i = 0; i < entryCount; i++)
            {
                if (id == ids[i] && r.Equals(entries[i]))
                {
                    return i;
                }
            }

            return -1;
        }

        internal bool isLeaf()
        {
            return level == 1;
        }

        // oldRectangle is a rectangle that has just been deleted or made smaller.
        // Thus, the MBR is only recalculated if the OldRectangle influenced the old MBR
        internal void recalculateMBR(Rectangle deletedRectangle)
        {
            if (mbr.EdgeOverlaps(deletedRectangle))
            {
                mbr.Set(entries[0]._min, entries[0]._max);

                for (var i = 1; i < entryCount; i++)
                {
                    mbr.Add(entries[i]);
                }
            }
        }

        /**
         * eliminate null entries, move all entries to the start of the source node
         */
        internal void reorganize([NotNull] RTree<T> rtree)
        {
            var countdownIndex = rtree.maxNodeEntries - 1;
            for (var index = 0; index < entryCount; index++)
            {
                if (entries[index] == null)
                {
                    while (entries[countdownIndex] == null && countdownIndex > index)
                    {
                        countdownIndex--;
                    }

                    entries[index] = entries[countdownIndex];
                    ids[index] = ids[countdownIndex];
                    entries[countdownIndex] = null;
                }
            }
        }
    }
}