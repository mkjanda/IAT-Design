function GenerateEventList() { 
    var iatBlock, instructionBlock, IATBlocks = new Array(), InstructionBlocks = new Array(), NumItemsAry = new Array(), piFunctions = new Array(), pifAry, ctr2, ctr3, randomNum, sourceAry = 1, iatItem, lesserAry, bAlternate, itemBlockOrder, instructionBlockOrder, itemBlockCtr = 0, instructionBlockCtr = 0, ndx; 
    EventCtr = 0; 
    bAlternate = (CookieUtil.get("Alternate") == "yes") ? true : false; 
    NumItemsAry.push(36);
    pifAry = new Array(); 
    piFunctions.push(pifAry);
    pifAry.push(PDIF1_1_1); 
    pifAry.push(PDIF1_1_2); 
    pifAry.push(PDIF1_1_3); 
    pifAry.push(PDIF1_1_4); 
    pifAry.push(PDIF1_1_5); 
    pifAry.push(PDIF1_1_6); 
    pifAry.push(PDIF1_1_7); 
    pifAry.push(PDIF1_1_8); 
    iatBlock = new IATBlock(1, 1, 36, -1); 
    iatBlock.BeginBlockEvent = new IATBeginBlock(false, DI17, DI18, DI19); 
    IATBlocks.push(iatBlock); 
    NumItemsAry.push(14); 
    pifAry = new Array(); 
    piFunctions.push(pifAry); 
    pifAry.push(PIF2);
    iatBlock = new IATBlock(2, 3, 14, -1); 
    iatBlock.BeginBlockEvent = new IATBeginBlock(false, DI60, DI61, DI62); 
    IATBlocks.push(iatBlock);
    NumItemsAry.push(50); 
    pifAry = new Array();
    piFunctions.push(pifAry); 
    pifAry.push(PDIF3_1_1); 
    pifAry.push(PDIF3_1_2); 
    pifAry.push(PDIF3_1_3); 
    pifAry.push(PDIF3_1_4); 
    pifAry.push(PDIF3_1_5); 
    pifAry.push(PDIF3_1_6); 
    pifAry.push(PDIF3_1_7); 
    pifAry.push(PDIF3_1_8); 
    pifAry.push(PIF3); 
    iatBlock = new IATBlock(3, 5, 50, 6); 
    iatBlock.BeginBlockEvent = new IATBeginBlock(false, DI81, DI82, DI83); 
    IATBlocks.push(iatBlock);
    NumItemsAry.push(50); 
    pifAry = new Array(); 
    piFunctions.push(pifAry); 
    pifAry.push(PDIF4_1_1); 
    pifAry.push(PDIF4_1_2); 
    pifAry.push(PDIF4_1_3); 
    pifAry.push(PDIF4_1_4); 
    pifAry.push(PDIF4_1_5); 
    pifAry.push(PDIF4_1_6); 
    pifAry.push(PDIF4_1_7); 
    pifAry.push(PDIF4_1_8); 
    pifAry.push(PIF4); 
    iatBlock = new IATBlock(4, 6, 50, 7); 
    iatBlock.BeginBlockEvent = new IATBeginBlock(false, DI84, DI85, DI86); 
    IATBlocks.push(iatBlock); 
    NumItemsAry.push(14); 
    pifAry = new Array(); 
    piFunctions.push(pifAry); 
    pifAry.push(PIF5); 
    iatBlock = new IATBlock(5, 8, 14, -1); 
    iatBlock.BeginBlockEvent = new IATBeginBlock(false, DI91, DI92, DI93); 
    IATBlocks.push(iatBlock); 
    NumItemsAry.push(50); 
    pifAry = new Array(); 
    piFunctions.push(pifAry); 
    pifAry.push(PDIF6_1_1); 
    pifAry.push(PDIF6_1_2); 
    pifAry.push(PDIF6_1_3); 
    pifAry.push(PDIF6_1_4); 
    pifAry.push(PDIF6_1_5); 
    pifAry.push(PDIF6_1_6); 
    pifAry.push(PDIF6_1_7); 
    pifAry.push(PDIF6_1_8); 
    pifAry.push(PIF6); 
    iatBlock = new IATBlock(6, 10, 50, 3); 
    iatBlock.BeginBlockEvent = new IATBeginBlock(false, DI98, DI99, DI100); 
    IATBlocks.push(iatBlock); 
    NumItemsAry.push(50); 
    pifAry = new Array(); 
    piFunctions.push(pifAry); 
    pifAry.push(PDIF7_1_1); 
    pifAry.push(PDIF7_1_2); 
    pifAry.push(PDIF7_1_3); 
    pifAry.push(PDIF7_1_4); 
    pifAry.push(PDIF7_1_5); 
    pifAry.push(PDIF7_1_6); 
    pifAry.push(PDIF7_1_7); 
    pifAry.push(PDIF7_1_8); 
    pifAry.push(PIF7); 
    iatBlock = new IATBlock(7, 11, 50, 4); 
    iatBlock.BeginBlockEvent = new IATBeginBlock(false, DI101, DI102, DI103); 
    IATBlocks.push(iatBlock); 
    for (ctr = 0; ctr < 7; ctr++){ 
        Items1 = new Array(); 
        Items2 = new Array(); 
        sourceAry = ((sourceAry == 2) || (ctr == 0)) ? 1 : 2; 
        for (ctr2 = 0; ctr2 < processIATItemFunctions[ctr].length; ctr2++) 
            processIATItemFunctions[ctr][ctr2].call(); 
        if (Items1.length < Items2.length) 
            lesserAry = Items1; 
        else 
            lesserAry = Items2; 
        for (ctr2 = 0; ctr2 < NumItemsAry[ctr]; ctr2++){ 
            if (sourceAry == 1){ 
                iatItem = Items1[Math.floor(Math.random() * Items1.length)]; 
                sourceAry = 2; 
            } else{ 
                iatItem = Items2[Math.floor(Math.random() * Items2.length)]; 
                sourceAry = 1; 
            } 
            ItemBlocks[ctr].AddItem(iatItem); 
        } 
        IATBlocks[ctr].EndBlockEvent = new IATEndBlock(); 
    } 
    instructionBlock = new IATInstructionBlock(-1, 2); 
    instructionBlock = new IATInstructionBlock(-1, 4); 
    instructionBlock = new IATInstructionBlock(6, 6); 
    instructionBlock = new IATInstructionBlock(7, 7); 
    instructionBlock = new IATInstructionBlock(-1, 9); 
    instructionBlock = new IATInstructionBlock(3, 11); 
    instructionBlock = new IATInstructionBlock(4, 12); 
    itemBlockOrder = new Array(-1, -1, 6, 7, -1, 3, 4); 
    instructionBlockOrder = new Array(-1, -1, 5, -1, 3); 
    for (ctr = 0; ctr < 13; ctr++){ 
        if (ctr == ItemBlocks[itemBlockCtr].blockPosition){ 
            if (bAlternate) 
                ndx = (itemBlockOrder[itemBlockCtr++] == -1) ? itemBlockCtr: itemBlockOrder[itemBlockCtr] - 1; 
            else 
                ndx = itemBlockCtr++; 
            EventList.push(IATBlocks[ndx].BeginBlockEvent); 
            for (ctr2 = 0; ctr2 < IATBlocks[ndx].Items.length; ctr2++) 
                EventList.push(IATBlocks[ndx].Items[ctr2]); 
            EventList.push(IATBlocks[ndx].EndBlockEvent); 
        } else{ 
            if (bAlternate) 
                ndx = (instructionBlockOrder[instructionBlockCtr++] == -1) ? instructionBlockCtr : instructionBlockOrder[instructionBlockCtr] - 1; 
            else 
                ndx = instructionBlockCtr++; 
            for (ctr2 = 0; ctr2 < InstructionBlocks[ndx].screens.length; ctr2++) 
                EventList.push(InstructionBlocks[ndx].screens[ctr2]); 
        } 
    }
}