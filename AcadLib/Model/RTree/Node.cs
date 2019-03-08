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
namespace RTreeLib
{
    using JetBrains.Annotations;

    /**
     * <p>Used by RTree. There are no public methods in this class.</p>
     *
     * @author aled@sourceforge.net
     * @version 1.0b2p1
     */
    [PublicAPI]
    public class Node<T>
    {
        internal Rectangle[] entries;
        internal int entryCount;
        internal int[] ids;
        internal int level;
        internal Rectangle mbr;
        internal int nodeId;

        public Node(int nodeId, int level, int maxNodeEntries)
        {
            this.nodeId = nodeId;
            this.level = level;
            entries = new Rectangle[maxNodeEntries];
            ids = new int[maxNodeEntries];
        }

        [CanBeNull]
        public Rectangle GetEntry(int index)
        {
            return index < entryCount ? entries[index] : null;
        }

        public int GetEntryCount()
        {
            return entryCount;
        }

        public int GetId(int index)
        {
            if (index < entryCount)
            {
                return ids[index];
            }

            return -1;
        }

        public int GetLevel()
        {
            return level;
        }

        public Rectangle GetMBR()
        {
            return mbr;
        }

        internal void AddEntry([NotNull] Rectangle r, int id)
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

        internal void AddEntryNoCopy(Rectangle r, int id)
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
        internal void DeleteEntry(int i, int minNodeEntries)
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
                RecalculateMBR(deletedRectangle);
            }
        }

        // Return the index of the found entry, or -1 if not found
        internal int FindEntry(Rectangle r, int id)
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

        internal bool IsLeaf()
        {
            return level == 1;
        }

        // oldRectangle is a rectangle that has just been deleted or made smaller.
        // Thus, the MBR is only recalculated if the OldRectangle influenced the old MBR
        internal void RecalculateMBR(Rectangle deletedRectangle)
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
#pragma warning disable 618

        internal void Reorganize([NotNull] RTree<T> rtree)
#pragma warning restore 618
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