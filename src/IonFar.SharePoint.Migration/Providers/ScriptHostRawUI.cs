using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Host;
using System.Text;
using System.Threading.Tasks;

namespace IonFar.SharePoint.Migration.Providers
{
    class ScriptHostRawUI : PSHostRawUserInterface
    {
        /*
https://msdn.microsoft.com/en-us/library/ee706570(v=vs.85).aspx

BackgroundColor
BufferSize
CursorSize
ForegroundColor
KeyAvailable
MaxPhysicalWindowSize
MaxWindowSize
WindowPosition
WindowSize
WindowTitle
FlushInputBuffer
        */

        public override ConsoleColor BackgroundColor
        {
            get
            {
                return ConsoleColor.Black;
                //throw new NotImplementedException("get_BackgroundColor not implemented");
            }

            set
            {
                throw new NotImplementedException("set_BackgroundColor not implemented");
            }
        }

        public override Size BufferSize
        {
            get
            {
                throw new NotImplementedException("get_BufferSize not implemented");
            }

            set
            {
                throw new NotImplementedException("set_BufferSize not implemented");
            }
        }

        public override Coordinates CursorPosition
        {
            get
            {
                throw new NotImplementedException("get_CursorPosition not implemented");
            }

            set
            {
                throw new NotImplementedException("set_CursorPosition not implemented");
            }
        }

        public override int CursorSize
        {
            get
            {
                throw new NotImplementedException("get_CursorSize not implemented");
            }

            set
            {
                throw new NotImplementedException("set_CursorSize not implemented");
            }
        }

        public override ConsoleColor ForegroundColor
        {
            get
            {
                return ConsoleColor.White;
                //throw new NotImplementedException("get_ForegroundColor not implemented");
            }

            set
            {
                throw new NotImplementedException("set_ForegroundColor not implemented");
            }
        }

        public override bool KeyAvailable
        {
            get
            {
                throw new NotImplementedException("get_KeyAvailable not implemented");
            }
        }

        public override Size MaxPhysicalWindowSize
        {
            get
            {
                throw new NotImplementedException("get_MaxPhysicalWindowSize not implemented");
            }
        }

        public override Size MaxWindowSize
        {
            get
            {
                throw new NotImplementedException("get_MaxWindowSize not implemented");
            }
        }

        public override Coordinates WindowPosition
        {
            get
            {
                throw new NotImplementedException("get_WindowPosition not implemented");
            }

            set
            {
                throw new NotImplementedException("set_WindowPosition not implemented");
            }
        }

        public override Size WindowSize
        {
            get
            {
                throw new NotImplementedException("get_WindowSize not implemented");
            }

            set
            {
                throw new NotImplementedException("set_WindowSize not implemented");
            }
        }

        public override string WindowTitle
        {
            get
            {
                throw new NotImplementedException("get_WindowTitle not implemented");
            }

            set
            {
                throw new NotImplementedException("set_WindowTitle not implemented");
            }
        }

        public override void FlushInputBuffer()
        {
            throw new NotImplementedException("FlushInputBuffer not implemented");
        }

        public override BufferCell[,] GetBufferContents(Rectangle rectangle)
        {
            throw new NotImplementedException("GetBufferContents not implemented");
        }

        public override KeyInfo ReadKey(ReadKeyOptions options)
        {
            throw new NotImplementedException("ReadKey not implemented");
        }

        public override void ScrollBufferContents(Rectangle source, Coordinates destination, Rectangle clip, BufferCell fill)
        {
            throw new NotImplementedException("ScrollBufferContents not implemented");
        }

        public override void SetBufferContents(Rectangle rectangle, BufferCell fill)
        {
            throw new NotImplementedException("SetBufferContents not implemented");
        }

        public override void SetBufferContents(Coordinates origin, BufferCell[,] contents)
        {
            throw new NotImplementedException("SetBufferContents (multiple) not implemented");
        }
    }
}
