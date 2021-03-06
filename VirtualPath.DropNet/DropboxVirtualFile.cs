﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DropNet.Models;
using VirtualPath.Common;

namespace VirtualPath.DropNet
{
    public class DropboxVirtualFile : AbstractVirtualFileBase
    {
        private DropboxVirtualPathProvider Provider
        {
            get { return ((DropboxVirtualPathProvider)VirtualPathProvider); }
        }

        private MetaData MetaData
        {
            get
            {
                return Provider.GetMetadata(this.VirtualPath);
            }
        }

        public DropboxVirtualFile(DropboxVirtualPathProvider owningProvider, DropboxVirtualDirectory directory, string name)
            : base(owningProvider, directory)
        {
            _name = name;
        }

        private string _name;
        public override string Name
        {
            get { return _name; }
        }

        public override DateTime LastModified
        {
            get { return MetaData.ModifiedDate; }
        }

        public override System.IO.Stream OpenRead()
        {
            return Provider.OpenRead(this.VirtualPath);
        }

        public override System.IO.Stream OpenWrite(WriteMode mode)
        {
            return Provider.OpenWrite(this.Directory.VirtualPath, this.Name, mode);
        }

        protected override IVirtualFile CopyBackingFileToDirectory(IVirtualDirectory directory, string name)
        {
            if (directory is DropboxVirtualDirectory)
            {
                var dir = (DropboxVirtualDirectory)directory;
                if (dir.Provider == this.Provider)
                {
                    Provider.Copy(this.VirtualPath, Provider.CombineVirtualPath(directory.VirtualPath, name));
                    return new DropboxVirtualFile(Provider, dir, name);
                }
            }

            // TODO: copy cross Dropboxes using CopyRef?

            return directory.CopyFile(this, name);
        }

        protected override IVirtualFile MoveBackingFileToDirectory(IVirtualDirectory directory, string name)
        {
            if (directory is DropboxVirtualDirectory)
            {
                var dir = (DropboxVirtualDirectory)directory;
                if (dir.Provider == this.Provider)
                {
                    Provider.Move(this.VirtualPath, Provider.CombineVirtualPath(directory.VirtualPath, name));
                    ((DropboxVirtualDirectory)Directory).RemoveFromContents(this);
                    return new DropboxVirtualFile(Provider, dir, name);
                }
            }

            var newFile = directory.CopyFile(this, name);
            this.Delete();
            return newFile;
        }
    }
}
