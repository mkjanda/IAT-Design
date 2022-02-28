using System;
using System.Collections.Generic;

namespace IATClient
{
    public interface IValidatedItem
    {
        void ValidateItem(Dictionary<IValidatedItem, CValidationException> ErrorDictionary);
    }

    public enum EValidationException
    {
        BlockResponseKeyUndefined, ItemKeyedDirUndefined, ItemStimulusUndefined, ImageStimulusIncompletelyInitialized, TextStimlusIncompletelyInitialized,
        InstructionScreenWithoutType, KeyInstructionScreenWithoutResponseKey, MockItemScreenWithoutResponseKey, MockItemScreenWithoutStimulus, MockItemScreenWithIncompletelyInitializedTextStimulus,
        MockItemScreenWithIncompletelyInitializedImageStimulus, TextInstructionsBlank, ContinueInstructionsBlank, BlockHasNoItems, InstructionBlockHasNoItems

    };

    public abstract class CLocationDescriptor
    {
        public abstract String ContainerNum { get; }
        public abstract String ItemNum { get; }

        public abstract void OpenItem(IATConfigMainForm mainForm);
    }


    public class CItemLocationDescriptor : CLocationDescriptor
    {
        CIATBlock Block = null;
        CIATItem Item = null;

        public override String ContainerNum
        {
            get
            {
                return (Block.IndexInContainer + 1).ToString();
            }
        }

        public override String ItemNum
        {
            get
            {
                if (Item == null)
                    return (-1).ToString();
                return (Block.GetItemIndex(Item) + 1).ToString();
            }
        }

        public CItemLocationDescriptor(CIATBlock block, CIATItem item)
        {
            Block = block;
            Item = item;
        }

        public override void OpenItem(IATConfigMainForm mainForm)
        {
            mainForm.ActiveItem = Block;
            mainForm.FormContents = IATConfigMainForm.EFormContents.IATBlock;
            if (Item != null)
                mainForm.SetActiveIATItem(Item);
        }
    }

    class CInstructionLocationDescriptor : CLocationDescriptor
    {
        CInstructionBlock InstructionBlock = null;
        CInstructionScreen InstructionScreen = null;

        public override String ContainerNum
        {
            get
            {
                return (InstructionBlock.IndexInContainer + 1).ToString();
            }
        }

        public override String ItemNum
        {
            get
            {
                if (InstructionScreen == null)
                    return (-1).ToString();
                return (InstructionScreen.IndexInContainer + 1).ToString();
            }
        }

        public CInstructionLocationDescriptor(CInstructionBlock instructionBlock, CInstructionScreen instructionScreen)
        {
            InstructionBlock = instructionBlock;
            InstructionScreen = instructionScreen;
        }

        public override void OpenItem(IATConfigMainForm mainForm)
        {
            mainForm.ActiveItem = InstructionBlock;
            mainForm.FormContents = IATConfigMainForm.EFormContents.Instructions;
            if (InstructionScreen != null)
                mainForm.SetActiveInstructionScreen(InstructionScreen);
        }
    }


    public class CValidationException : Exception
    {
        private EValidationException _Type;
        private CLocationDescriptor _LocationDescriptor;

        public EValidationException Type
        {
            get
            {
                return _Type;
            }
        }

        public CLocationDescriptor LocationDescriptor
        {
            get
            {
                return _LocationDescriptor;
            }
        }

        public CValidationException(EValidationException type, CLocationDescriptor locationDescriptor)
        {
            _Type = type;
            _LocationDescriptor = locationDescriptor;
        }

        public CValidationException(String message) : base(message) { }

        public String ErrorText
        {
            get
            {
                switch (Type)
                {
                    case EValidationException.BlockHasNoItems:
                        return String.Format(Properties.Resources.sBlockHasNoItems, LocationDescriptor.ContainerNum);

                    case EValidationException.BlockResponseKeyUndefined:
                        return String.Format(Properties.Resources.sBlockLacksResponseKey, LocationDescriptor.ContainerNum);

                    case EValidationException.ImageStimulusIncompletelyInitialized:
                        return String.Format(Properties.Resources.sImageStimulusImproperlyDefined, LocationDescriptor.ContainerNum, LocationDescriptor.ItemNum);

                    case EValidationException.InstructionBlockHasNoItems:
                        return String.Format(Properties.Resources.sInstructionBlockHasNoItems, LocationDescriptor.ContainerNum);

                    case EValidationException.InstructionScreenWithoutType:
                        return String.Format(Properties.Resources.sInstructionScreenTypeNotSet, LocationDescriptor.ContainerNum, LocationDescriptor.ItemNum);

                    case EValidationException.ItemKeyedDirUndefined:
                        return String.Format(Properties.Resources.sItemLacksKeyedDir, LocationDescriptor.ContainerNum, LocationDescriptor.ItemNum);

                    case EValidationException.ItemStimulusUndefined:
                        return String.Format(Properties.Resources.sItemLacksStimulus, LocationDescriptor.ContainerNum, LocationDescriptor.ItemNum);

                    case EValidationException.KeyInstructionScreenWithoutResponseKey:
                        return String.Format(Properties.Resources.sKeyInstructionScreenLacksResponseKey, LocationDescriptor.ContainerNum, LocationDescriptor.ItemNum);

                    case EValidationException.MockItemScreenWithIncompletelyInitializedImageStimulus:
                        return String.Format(Properties.Resources.sMockItemImageStimulusImproperlyDefined, LocationDescriptor.ContainerNum, LocationDescriptor.ItemNum);

                    case EValidationException.MockItemScreenWithIncompletelyInitializedTextStimulus:
                        return String.Format(Properties.Resources.sMockItemTextStimulusImproperlyDefined, LocationDescriptor.ContainerNum, LocationDescriptor.ItemNum);

                    case EValidationException.MockItemScreenWithoutResponseKey:
                        return String.Format(Properties.Resources.sMockItemLacksResponseKey, LocationDescriptor.ContainerNum, LocationDescriptor.ItemNum);

                    case EValidationException.MockItemScreenWithoutStimulus:
                        return String.Format(Properties.Resources.sMockItemLacksStimulus, LocationDescriptor.ContainerNum, LocationDescriptor.ItemNum);

                    case EValidationException.TextStimlusIncompletelyInitialized:
                        return String.Format(Properties.Resources.sTextStimulusImproperlyDefined, LocationDescriptor.ContainerNum, LocationDescriptor.ItemNum);

                    case EValidationException.TextInstructionsBlank:
                        return String.Format(Properties.Resources.sTextInstructionsBlank, LocationDescriptor.ContainerNum, LocationDescriptor.ItemNum);

                    case EValidationException.ContinueInstructionsBlank:
                        return String.Format(Properties.Resources.sContinueInstructionsBlank, LocationDescriptor.ContainerNum, LocationDescriptor.ItemNum);

                    default:
                        return String.Empty;
                }
            }
        }
    }

    static class CItemValidator
    {
        private static Dictionary<IValidatedItem, CValidationException> _ErrorDictionary = new Dictionary<IValidatedItem, CValidationException>();

        public static Dictionary<IValidatedItem, CValidationException> ErrorDictionary
        {
            get
            {
                return _ErrorDictionary;
            }
        }

        public static void StartValidation()
        {
            ErrorDictionary.Clear();
        }

        public static void ValidateItem(IValidatedItem i)
        {
            i.ValidateItem(ErrorDictionary);
        }

        public static bool HasErrors
        {
            get
            {
                return ErrorDictionary.Count > 0;
            }
        }

        public static void DisplayErrors(IATConfigMainForm mainForm)
        {
            ErrorForm errorForm = new ErrorForm(ErrorDictionary, mainForm);
            errorForm.ShowDialog(mainForm);
        }
    }
}
