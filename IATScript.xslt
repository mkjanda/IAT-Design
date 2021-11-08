<xsl:stylesheet
    xmlns:xs="http://www.w3.org/2001/XMLSchema"
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0"
    exclude-result-prefixes="xs">
  <xsl:output method="text" encoding="us-ascii" indent="no" />
  <xsl:variable name="root" select="/" />

  <xsl:template match="ConfigFile" >

    <xsl:text>
    var inPracticeBlock = false;
    var currentItemKeyedDir = "Left";
    var currentContinueKeyCode = 32;
    var isErrorMarked = false;
    var currentHandler = null;
    var currentStimulus = null;
    var currentItemNum = 0;
    var currentItemID;
    var EventList = new Array();
    var EventCtr = 0;
    var ErrorMark;
    var ImageLoadCtr = 0;
    var NumImages = </xsl:text><xsl:value-of select="./DisplayItemList/@NumDisplayItems" /><xsl:text>;
    var ImageLoadStatusTextElement;
    var ClickToStartElement;
    var ClickToStartText;
    var KeyedDir;
    var KeyedDirArray;
    var OriginatingBlockArray;
    var StimulusIDArray;
    var ItemNumArray;
    var UsedMaskItems;
    var InUsedMaskedItems;
    var randomNum;
    var RandomItem;
    var SelectedItem;
    var KeyedDirInput;
	var MaskItemTrueArray;
  var Display;
	var MaskItemFalseArray;
    var DefaultKey;
    var SelectedNum;
    var FreeItemIDs;
    var SelectionSpecNdx;
    var Items;
    var ndx1, ndx2;
    var Items1;

    var Items2;
    var ctr;
    var EventUtil = {
    addHandler: function(element, type, handler) {
    if (element.addEventListener) {
    element.addEventListener(type, handler, false);
    } else if (element.attachEvent) {
    element.attachEvent("on" + type, handler);
    } else {
    element["on" + type] = handler;
    }
    },

    getEvent: function(event) {
    return event ? event : window.event;
    },

    getTarget: function(event) {
    return event.target || event.srcElement;
    },

    preventDefault: function(event) {
    if (event.preventDefault) {
    event.preventDefault();
    } else {
    event.returnValue = false;
    }
    },

    removeHandler: function(element, type, handler) {
    if (element.removeEventListener) {
    element.removeEventListener(type, handler, false);
    }
    else if (element.detachEvent) {
    element.detachEvent("on" + type, handler);
    } else {
    element["on" + type] = null;
    }
    },

    stopPropagation: function(event) {
    if (event.stopPropagation) {
    event.stopPropagation();
    } else {
    event.cancelBubble = true;
    }
    },

    getCharCode: function(event) {
    if (typeof event.charCode == "number") {
    return event.charCode;
    } else {
    return event.keyCode;
    }
    }
    };

	</xsl:text>
    <xsl:for-each select="./DisplayItemList/*">
      <xsl:value-of select="concat('var DisplayItem', ./ID, ';&#x0A;')" />
    </xsl:for-each>
    <xsl:text>
    var SelectionStimulusArray;

    var SelectionKeyedDir;

	var SelectionOriginatingBlock;

    var SelectionStimulus;

    var SelectionItemNum;

    function OnImageLoad(){
    ImageLoadCtr++;
    if (ImageLoadCtr == NumImages)
      OnImageLoadComplete();
    else 
      ImageLoadStatusTextElement.nodeValue = "Loading image #" + (ImageLoadCtr + 1).toString() + " of " + NumImages.toString();
    }

    function IATDisplayItem(id, src, x, y, width, height)
    {
    this.id = id;
    this.src = </xsl:text><xsl:value-of select="concat('&quot;', ./ServerURL, ./ClientID, '/', ./IATName, '/&quot; + src;')"/><xsl:text>
    this.x = x;
    this.y = y;
    this.width = width;
    this.height = height;
    this.img = new Image();
    this.imgTag = document.createElement("img");
    this.imgTag.appendChild(this.img);
    this.imgTag.id = "IATDisplayItem" + id.toString();
    }

    IATDisplayItem.prototype = {
    constructor: IATDisplayItem,
    Load : function() {
    EventUtil.addHandler(this.img, "load", OnImageLoad);
    this.img.src = this.src;
    },

    Outline : function()
    {
    this.imgTag.className = "outlinedDI";
    },

    Display : function(parentNode) {
    this.imgTag.src = this.src;
    parentNode.appendChild(this.imgTag);
    },

    Hide : function() {
    if (this.imgTag.parentNode) {
    this.imgTag.parentNode.removeChild(this.imgTag);
    }
    this.imgTag.className = "";
    }
    };

    function IATDisplay(interiorWidth, interiorHeight, leftResponseKeyCodeUpper, leftResponseKeyCodeLower, rightResponseKeyCodeUpper, rightResponseKeyCodeLower)
    {
    this.interiorWidth = interiorWidth;
    this.interiorHeight = interiorHeight;
    this.leftResponseKeyCodeUpper = leftResponseKeyCodeUpper;
    this.leftResponseKeyCodeLower = leftResponseKeyCodeLower;
    this.rightResponseKeyCodeUpper = rightResponseKeyCodeUpper;
    this.rightResponseKeyCodeLower = rightResponseKeyCodeLower;
    this.startTime = -1;

    this.divTag = document.getElementById("IATDisplayDiv");

    this.displayItems = new Array();
    }

    

    IATDisplay.prototype = {
    constructor : IATDisplay,
    AddDisplayItem : function(di) {
    this.displayItems[this.displayItems.length] = di;
    di.Display(this.divTag);
    },

    RemoveDisplayItem : function(di) {
    for (var ctr = 0; ctr &lt; this.displayItems.length; ctr++)
    {
    if (this.displayItems[ctr].id == di.id)
    {
    this.displayItems[ctr].Hide();
    this.displayItems.splice(ctr, 1);
    }
    }
    },

    Clear : function() {
    for (var ctr = 0; ctr &lt; this.displayItems.length; ctr++)
    this.displayItems[ctr].Hide();
    this.displayItems.splice(0, this.displayItems.length);
    },

    StartTimer : function() {
    this.startTime = (new Date()).getTime();
    }
    };

    function IATEvent(id, handler)
    {
    this.id = id;
    this.handler = handler;
    }

    IATEvent.prototype = {
    constructor : IATEvent,
    Execute : function() {
    if (this.handler == null)
    EventList[++EventCtr].Execute();
    else {
    currentHandler = this.handler;
    EventUtil.addHandler(Display.divTag, "keypress", this.handler);
    }
    }
    };

    function IATSubmitEvent(id)
    {
    IATEvent.call(this, id, null);
    }

    IATSubmitEvent.prototype = {
    constructor : IATSubmitEvent,
    Execute : function() {
    var numItemsInput = document.createElement("input");
    numItemsInput.name = "NumItems";
    numItemsInput.type = "hidden";
    numItemsInput.value = currentItemNum.toString();
    Display.divTag.appendChild(numItemsInput);
    var form = document.getElementById("IATForm");
    form.submit();
    }};


    function IATItem(id, stimulus, itemNum, keyedDir)
    {
    if (keyedDir == "Left")
    {
    IATEvent.call(this, id, function(event)
    {
    event = EventUtil.getEvent(event);
    var keyCode = EventUtil.getCharCode(event);
    if ((keyCode == Display.leftResponseKeyCodeUpper) || (keyCode == Display.leftResponseKeyCodeLower))
    {
    if (!inPracticeBlock)
    {
    var latencyItemName = document.createElement("input");
    latencyItemName.name = "ItemNum" + currentItemNum.toString();
    latencyItemName.value = itemNum.toString();
    latencyItemName.type = "hidden";
    Display.divTag.appendChild(latencyItemName);
    var latency = (new Date()).getTime() - Display.startTime;
    var latencyOutput = document.createElement("input");
    latencyOutput.name = "Item" + currentItemNum.toString();
    currentItemNum++;
    latencyOutput.type = "hidden";
    latencyOutput.value = latency.toString();
    Display.divTag.appendChild(latencyOutput);
    }
    if (isErrorMarked)
    Display.RemoveDisplayItem(ErrorMark);
    isErrorMarked = false;
    Display.RemoveDisplayItem(currentStimulus);
    EventUtil.removeHandler(Display.divTag, "keypress", currentHandler);
    EventList[++EventCtr].Execute();
    }
    else if ((keyCode == Display.rightResponseKeyCodeUpper) || (keyCode == Display.rightResponseKeyCodeLower))
    {
    if (!isErrorMarked) {
    Display.AddDisplayItem(ErrorMark);
    isErrorMarked = true;
    }
    }
    });
    }
    else {
    IATEvent.call(this, id, function(event)
    {
    event = EventUtil.getEvent(event);
    var keyCode = EventUtil.getCharCode(event);
    if ((keyCode == Display.rightResponseKeyCodeUpper) || (keyCode == Display.rightResponseKeyCodeLower))
    {
    if (!inPracticeBlock)
    {
    var latencyItemName = document.createElement("input");
    latencyItemName.name = "ItemNum" + currentItemNum.toString();
    latencyItemName.value = itemNum.toString();
    latencyItemName.type = "hidden";
    Display.divTag.appendChild(latencyItemName);
    var latency = (new Date()).getTime() - Display.startTime;
    var latencyOutput = document.createElement("input");
    latencyOutput.name = "Item" + currentItemNum.toString();
    currentItemNum++;
    latencyOutput.type = "hidden";
    latencyOutput.value = latency.toString();
    Display.divTag.appendChild(latencyOutput);
    }
    if (isErrorMarked)
    Display.RemoveDisplayItem(ErrorMark);
    isErrorMarked = false;
    Display.RemoveDisplayItem(currentStimulus);
    EventUtil.removeHandler(Display.divTag, "keypress", currentHandler);
    EventList[++EventCtr].Execute();
    }
    else if ((keyCode == Display.leftResponseKeyCodeUpper) || (keyCode == Display.leftResponseKeyCodeLower))
    {
    if (!isErrorMarked) {
    Display.AddDisplayItem(ErrorMark);
    isErrorMarked = true;
    }
    }

    });
    }
    this.isErrorMarked = false;
    this.stimulus = stimulus;
    this.itemNum = itemNum;
    }

    IATItem.prototype = {
    constructor : IATItem,
    Execute : function() {
    currentItemKeyedDir = this.keyedDir;
    currentStimulus = this.stimulus;
    currentItemID = this.itemNum;
    Display.AddDisplayItem(this.stimulus);
    Display.StartTimer();
    while ((new Date()).getTime() - 100 &lt; Display.startTime);
    IATEvent.prototype.Execute.call(this);
    }
    };

    function IATBeginBlock(id, isPracticeBlock, leftDisplayItem, rightDisplayItem, instructionsDisplayItem)
    {
    IATEvent.call(this, id, null);
    this.isPracticeBlock = isPracticeBlock;
    this.leftDisplayItem = leftDisplayItem;
    this.rightDisplayItem = rightDisplayItem;
    this.instructionsDisplayItem = instructionsDisplayItem;
    }

    IATBeginBlock.prototype = {
    constructor : IATBeginBlock,
    Execute : function() {
    inPracticeBlock = this.isPracticeBlock;
    Display.AddDisplayItem(this.leftDisplayItem);
    Display.AddDisplayItem(this.rightDisplayItem);
    Display.AddDisplayItem(this.instructionsDisplayItem);
    IATEvent.prototype.Execute.call(this);
    }
    };

    function IATEndBlock(id)
    {
    IATEvent.call(this, id, null);
    }

    IATEndBlock.prototype = {
    constructor : IATEndBlock,
    Execute : function() {
    inPracticeBlock = false;
    Display.Clear();
    IATEvent.prototype.Execute.call(this);
    }
    };

    function IATInstructionScreen(id, continueChar, continueInstructionsDI)
    {
    IATEvent.call(this, id, function(event) {
    event = EventUtil.getEvent(event);
    var keyCode = EventUtil.getCharCode(event);
    if (keyCode == currentContinueKeyCode)
    {
    Display.Clear();
    EventUtil.removeHandler(Display.divTag, "keypress", currentHandler);
    IATEvent.prototype.Execute.call(this);
    }
    });

    this.continueChar = continueChar;
    this.continueInstructionsDI = continueInstructionsDI;
    }

    IATInstructionScreen.prototype =
    {
    constructor : IATInstructionScreen,
    Execute : function()
    {
    Display.AddDisplayItem(this.continueInstructionsDI);
    currentContinueKeyCode = this.continueChar;
    IATEvent.prototype.Execute.call(this);
    }
    };

    function IATTextInstructionScreen(id, continueChar, continueInstructionsDI, textInstructionsDI)
    {
    IATInstructionScreen.call(this, id, continueChar, continueInstructionsDI);
    this.textInstructionsDI = textInstructionsDI;
    }

    IATTextInstructionScreen.prototype = {
    constructor : IATTextInstructionScreen,
    Execute : function()
    {
    Display.AddDisplayItem(this.textInstructionsDI);
    IATInstructionScreen.prototype.Execute.call(this);
    }
    };

    function IATMockItemInstructionScreen(id, continueChar, continueInstructionsDI, leftResponseDI, rightResponseDI, stimulusDI, instructionsDI, errorMarked, outlineLeftResponse, outlineRightResponse)
    {
    IATInstructionScreen.call(this, id, continueChar, continueInstructionsDI);
    this.leftResponseDI = leftResponseDI;
    this.rightResponseDI = rightResponseDI;
    this.stimulusDI = stimulusDI;
    this.instructionsDI = instructionsDI;
    this.errorMarked = errorMarked;
    this.outlineLeftResponse = outlineLeftResponse;
    this.outlineRightResponse = outlineRightResponse;
    }

    IATMockItemInstructionScreen.prototype = {
    constructor : IATMockItemInstructionScreen,
    Execute : function()
    {
    if (this.outlineLeftResponse)
    this.leftResponseDI.Outline();
    Display.AddDisplayItem(this.leftResponseDI);
    if (this.outlineRightResponse)
    this.rightResponseDI.Outline();
    Display.AddDisplayItem(this.rightResponseDI);
    Display.AddDisplayItem(this.stimulusDI);
    Display.AddDisplayItem(this.instructionsDI);
    if (this.errorMarked)
    Display.AddDisplayItem(ErrorMark);
    IATInstructionScreen.prototype.Execute.call(this);
    }
    };

    function IATKeyedInstructionScreen(id, continueChar, continueInstructionsDI, instructionsDI, leftResponseDI, rightResponseDI)
    {
    IATInstructionScreen.call(this, id, continueChar, continueInstructionsDI);
    this.instructionsDI = instructionsDI;
    this.leftResponseDI = leftResponseDI;
    this.rightResponseDI = rightResponseDI;
    }

    IATKeyedInstructionScreen.prototype = {
    constructor : IATKeyedInstructionScreen,
    Execute : function()
    {
    Display.AddDisplayItem(this.instructionsDI);
    Display.AddDisplayItem(this.leftResponseDI);
    Display.AddDisplayItem(this.rightResponseDI);
    IATInstructionScreen.prototype.Execute.call(this);
    }
    };

    function IATBlock(blockNum, numPresentations, alternatedWith)
    {
    this.blockNum = blockNum;
    this.numPresentations = numPresentations;
    this.alternatedWith = alternatedWith;
    this.BeginBlockEvent = null;
    this.EndBlockEvent = null;
    this.Items = new Array();
    }

    IATBlock.prototype = {
    constructor : IATBlock,
    AddItem : function(item)
    {
    this.Items.push(item);
    },

    GenerateContents : function(randomization)
    {
    var result = new Array();
    result.push(this.BeginBlockEvent);
    var ctr;
    var currItemNdx, lastItemNdx = -1;
    if (randomization == "None")
    {
    for (ctr = 0; ctr &lt; Items.length; ctr++)
    result.push(this.Items[ctr]);
    }
    else if (randomization == "RandomOrder")
    {
    var tempItems = new Array();
    for (ctr = 0; ctr &lt; this.Items.length; ctr++)
    tempItems.push(this.Items[ctr]);
    for (ctr = 0; ctr &lt; this.Items.length; ctr++)
    {
    var ndx = Math.floor(Math.random() * tempItems.length);
    result.push(tempItems[ndx]);
    tempItems.splice(ndx, 1);
    }
    } else if (randomization == "SetNumberOfPresentations")
    {
    for (ctr = 0; ctr &lt; this.numPresentations; ctr++)
    {
    currItemNdx = Math.floor(Math.random() * this.Items.length);
    while (currItemNdx == lastItemNdx)
    currItemNdx = Math.floor(Math.random() * this.Items.length);
    result.push(this.Items[currItemNdx]);
    lastItemNdx = currItemNdx;
    }
    }
    result.push(this.EndBlockEvent);
    return result;
    }
    };

    function IATInstructionBlock(alternatedWith)
    {
    this.alternatedWith = alternatedWith;
    this.screens = new Array();
    }

    IATInstructionBlock.prototype = {
    constructor : IATInstructionBlock,
    AddScreen : function(screen) {
    this.screens.push(screen);
    }};

    function OnImageLoadComplete()
    {
    while (Display.divTag.firstChild)
    Display.divTag.removeChild(Display.divTag.firstChild);
    ClickToStartElement = document.createElement("h4");
    ClickToStartText = document.createTextNode("Click Here to Begin");
    ClickToStartElement.appendChild(ClickToStartText);
    Display.divTag.appendChild(ClickToStartElement);
    currentHandler = function() {
    Display.divTag.removeChild(ClickToStartElement);
</xsl:text>

    <xsl:call-template name="GenerateEventInit" >
      <xsl:with-param name="EventListNode" select="./IATEventList" />
      <xsl:with-param name="randomization" select="./RandomizationType" />
    </xsl:call-template>

    <xsl:text>
    EventUtil.removeHandler(Display.divTag, "click", currentHandler);
    EventList[EventCtr].Execute();
    }
    EventUtil.addHandler(Display.divTag, "click", currentHandler);
    Display.divTag.tabIndex = -1;
    Display.divTag.focus();
    var bodyTag = document.getElementById("bodyID");
    EventUtil.addHandler(bodyTag, "click", function() {
    Display.divTag.tabIndx = -1;
    Display.divTag.focus();
    });
    var containerDiv = document.getElementById("IATContainerDiv");
    EventUtil.addHandler(containerDiv, "click", function() {
    Display.divTag.tabIndex = -1;
    Display.divTag.focus();
    });
    }

    function BeginIATLoad()
    {
</xsl:text>
    <xsl:for-each select="./DisplayItemList/*" >
      <xsl:value-of select="concat('DisplayItem', ./ID)" /> = new IATDisplayItem(<xsl:value-of select="concat(./ID, ', &quot;', ./Filename, '&quot;, ', ./X, ', ', ./Y, ', ', ./Width, ', ', ./Height)" />);
    </xsl:for-each>
    Display = new IATDisplay(<xsl:value-of select="concat(./IATLayout/InteriorWidth, ', ', ./IATLayout/InteriorHeight, ', ', ./LeftResponseASCIIKeyCodeUpper, ', ', ./LeftResponseASCIIKeyCodeLower, ', ', ./RightResponseASCIIKeyCodeUpper, ', ', ./RightResponseASCIIKeyCodeLower)" /><xsl:text>);</xsl:text>
    <xsl:call-template name="GenerateImageLoad" >
      <xsl:with-param name="ImageListNode" select="./DisplayItemList" />
    </xsl:call-template>
    ErrorMark = <xsl:value-of select="concat('DisplayItem', ./ErrorMarkID)" />;
    ErrorMark.imgTag.id = <xsl:value-of select="concat('DisplayItem', ./ErrorMarkID)" />.imgTag.id;
    }

  </xsl:template>

  <xsl:template name="GenerateImageLoad" >
    <xsl:param name="ImageListNode" />
    <xsl:variable name="numImages" select="count($ImageListNode/*)" />
    ImageLoadCtr = 0;
    var LoadingImagesElement = document.createElement("h3");
    var LoadingImagesText = document.createTextNode("Please Wait");
    LoadingImagesElement.appendChild(LoadingImagesText);
    var LoadingImagesProgressElement = document.createElement("h4");
    ImageLoadStatusTextElement = document.createTextNode("");
    ImageLoadStatusTextElement.nodeValue = "Loading image #1 of " + NumImages.toString();
    LoadingImagesProgressElement.appendChild(ImageLoadStatusTextElement);
    Display.divTag.appendChild(LoadingImagesElement);
    Display.divTag.appendChild(LoadingImagesProgressElement);
    <xsl:for-each select="$ImageListNode/*" >
      <xsl:value-of select="concat('DisplayItem', ./ID, '.Load();&#x0A;')"/>
    </xsl:for-each>
  </xsl:template>

  <xsl:template name="GenerateEventInit" >
    <xsl:param name="EventListNode" />
    <xsl:param name="randomization" />
    var ItemBlocks = new Array();
    var InstructionBlocks = new Array();
    <xsl:for-each select="$EventListNode/IATEvent" >
      <xsl:choose>
        <xsl:when test="@EventType eq 'BeginIATBlock'" >

          <xsl:variable name="blockLength" select="./NumItems" as="xs:integer" />


          ItemBlocks.push(new IATBlock(ItemBlocks.length + 1, <xsl:value-of select="concat(./NumPresentations, ', ', ./AlternatedWith)" />));
          ItemBlocks[ItemBlocks.length - 1].BeginBlockEvent = new IATBeginBlock(<xsl:value-of select="concat(position(), ', ', lower-case(./PracticeBlock), ', DisplayItem', ./LeftResponseDisplayID, ', DisplayItem', ./RightResponseDisplayID, ', DisplayItem', ./InstructionsDisplayID)" />);

          <xsl:variable name="itemsInBlock" select="following::IATEvent[position() le $blockLength]" />
          <xsl:call-template name="GenerateBlockLoad" >
            <xsl:with-param name="items" select="$itemsInBlock" />
            <xsl:with-param name="startPosition" select="xs:integer(position())" as="xs:integer" />
          </xsl:call-template>

        </xsl:when>

        <xsl:when test="@EventType eq 'EndIATBlock'" >
          ItemBlocks[ItemBlocks.length - 1].EndBlockEvent = new IATEndBlock(<xsl:value-of select="position()" />);
        </xsl:when>
        <xsl:when test="@EventType eq 'BeginInstructionBlock'" >
          InstructionBlocks.push(new IATInstructionBlock(<xsl:value-of select="./AlternatedWith" />));
        </xsl:when>
        <xsl:when test="@EventType eq 'TextInstructionScreen'" >
          InstructionBlocks[InstructionBlocks.length - 1].AddScreen(new IATTextInstructionScreen(<xsl:value-of select="concat(position(), ', ', ./ContinueASCIIKeyCode, ', DisplayItem', ./ContinueInstructionsDisplayID, ', DisplayItem', ./InstructionsDisplayID)" />));
        </xsl:when>
        <xsl:when test="@EventType eq 'KeyedInstructionScreen'" >
          InstructionBlocks[InstructionBlocks.length - 1].AddScreen(new IATKeyedInstructionScreen(<xsl:value-of select="concat(position(), ', ', ./ContinueASCIIKeyCode, ', DisplayItem', ./ContinueInstructionsDisplayID, ', DisplayItem', ./InstructionsDisplayID, ', DisplayItem', ./LeftResponseDisplayID, ', DisplayItem', ./RightResponseDisplayID)" />));
        </xsl:when>
        <xsl:when test="@EventType eq 'MockItemInstructionScreen'" >
          InstructionBlocks[InstructionBlocks.length - 1].AddScreen(new IATMockItemInstructionScreen(<xsl:value-of select="concat(position(), ', ', ./ContinueASCIIKeyCode, ', DisplayItem', ./ContinueInstructionsDisplayID, ', DisplayItem', ./LeftResponseDisplayID, ', DisplayItem', ./RightResponseDisplayID, ', DisplayItem', ./StimulusDisplayID, ', DisplayItem', ./InstructionsDisplayID, ', ', lower-case(./ErrorMarkIsDisplayed), ', ', lower-case(./OutlineLeftResponse), ', ', lower-case(./OutlineRightResponse))" />));
        </xsl:when>
      </xsl:choose>
    </xsl:for-each>

    var itemBlock;
    var instructionBlock;
    var alternate = document.getElementById("Alternate").value;
    var itemBlockCtr = 0;
    var instructionBlockCtr = 0;
    var numAlternatedItemBlocks = 0;
    var numAlternatedInstructionBlocks = 0;
    <xsl:variable name="blockStarters" select="('BeginIATBlock', 'BeginInstructionBlock')" />
    <xsl:for-each select="$EventListNode/IATEvent[some $type in $blockStarters satisfies $type eq @EventType]" >
      <xsl:choose>
        <xsl:when test="@EventType eq 'BeginIATBlock'" >
          if (alternate == "yes") {
          <xsl:if test="./AlternatedWith ne '-1'" >
            <xsl:choose>
              <xsl:when test="./AlternatedWith gt ./BlockNum" >
                itemBlock = ItemBlocks[<xsl:value-of select="./AlternatedWith" /> - (itemBlockCtr + 1)];
                ItemBlocks.splice(<xsl:value-of select="./AlternatedWith" /> - (itemBlockCtr + 1), 1);
                numAlternatedItemBlocks++;
              </xsl:when>
              <xsl:when test="./AlternatedWith lt ./BlockNum" >
                itemBlock = ItemBlocks[<xsl:value-of select="./AlternatedWith" /> - (numAlternatedItemBlocks + 1)];
                ItemBlocks.splice(<xsl:value-of select="./AlternatedWith" /> - (numAlternatedItemBlocks + 1), 1);
                numAlternatedItemBlocks++;
              </xsl:when>
            </xsl:choose>
          </xsl:if>
          <xsl:if test="./AlternatedWith eq '-1'" >
            itemBlock = ItemBlocks[numAlternatedItemBlocks];
            ItemBlocks.splice(numAlternatedItemBlocks, 1);
          </xsl:if>
          }
          else
          itemBlock = ItemBlocks.shift();
          result = itemBlock.GenerateContents("<xsl:value-of select="$randomization" />");
          while (result.length &gt; 0)
          EventList.push(result.shift());
          itemBlockCtr++;
        </xsl:when>
        <xsl:when test="@EventType eq 'BeginInstructionBlock'" >
          <xsl:variable name="precedingNodes" select="preceding-sibling::node()" />
          <xsl:variable name="blockNum" select="count($precedingNodes[@EventType eq 'BeginInstructionBlock'])" as="xs:integer" />
          if (alternate == "yes") {
          <xsl:if test="./AlternatedWith ne '-1'" >
            <xsl:choose>
              <xsl:when test="xs:integer(./AlternatedWith) gt $blockNum" >
                instructionBlock = InstructionBlocks[<xsl:value-of select="./AlternatedWith" /> - (instructionBlockCtr + 1)];
                InstructionBlocks.splice(<xsl:value-of select="./AlternatedWith" /> - (instructionBlockCtr + 1), 1);
                numAlternatedInstructionBlocks++;
              </xsl:when>
              <xsl:when test="xs:integer(./AlternatedWith) lt $blockNum" >
                instructionBlock = InstructionBlocks[<xsl:value-of select="./AlternatedWith" /> - (numAlternatedInstructionBlocks + 2)];
                InstructionBlocks.splice(<xsl:value-of select="./AlternatedWith" /> - (numAlternatedInstructionBlocks + 2), 1);
                numAlternatedInstructionBlocks++;
              </xsl:when>
            </xsl:choose>
          </xsl:if>
          <xsl:if test="./AlternatedWith eq '-1'" >
            instructionBlock = InstructionBlocks[numAlternatedInstructionBlocks];
            InstructionBlocks.splice(numAlternatedInstructionBlocks, 1);
          </xsl:if>
          } else
          instructionBlock = InstructionBlocks.shift();
          while (instructionBlock.screens.length &gt; 0)
          EventList.push(instructionBlock.screens.shift());
          instructionBlockCtr++;
        </xsl:when>
      </xsl:choose>
    </xsl:for-each>
    EventList.push(new IATSubmitEvent(EventList.length));
  </xsl:template>

  <xsl:template name="GenerateBlockLoad" >
    <xsl:param name="items" />
    <xsl:param name="startPosition" as="xs:integer" />
    <xsl:choose>
      <xsl:when test="(//Is7Block eq 'True') and (//RandomizationType eq 'SetNumberOfPresentations')" >
        Items1 = new Array();
        Items2 = new Array();
      </xsl:when>
      <xsl:otherwise>
        Items = new Array();
      </xsl:otherwise>
    </xsl:choose>
    FreeItemIDs = new Array();
    <xsl:for-each select="$items" >
      <xsl:variable name="currentItem" select="." />
      <xsl:if test="xs:integer(./SpecifierID) eq -1" >
        KeyedDir = "<xsl:value-of select="./KeyedDir" />";
        <xsl:choose>
          <xsl:when test="(//Is7Block eq 'True') and (//RandomizationType eq 'SetNumberOfPresentations')" >
            <xsl:if test="$currentItem/OriginatingBlock eq '1'" >
              Items1.push(new IATItem(<xsl:value-of select="concat(xs:string(position() + $startPosition), ', ', 'DisplayItem', ./StimulusDisplayID, ', ', ./ItemNum, ',  KeyedDir')" />));
            </xsl:if>
            <xsl:if test="$currentItem/OriginatingBlock eq '2'" >
              Items2.push(new IATItem(<xsl:value-of select="concat(xs:string(position() + $startPosition), ', ', 'DisplayItem', ./StimulusDisplayID, ', ', ./ItemNum, ',  KeyedDir')" />));
            </xsl:if>
          </xsl:when>
          <xsl:otherwise>
            Items.push(new IATItem(<xsl:value-of select="concat(xs:string(position() + $startPosition), ', ', 'DisplayItem', ./StimulusDisplayID, ', ', ./ItemNum, ',  KeyedDir')" />));
          </xsl:otherwise>
        </xsl:choose>
      </xsl:if>
      <xsl:if test="xs:integer(./SpecifierID) ne -1" >
        <xsl:if test="./KeyedDir eq 'DynamicRight'" >
          DefaultKey = "Right";
        </xsl:if>
        <xsl:if test="./KeyedDir eq 'DynamicLeft'" >
          DefaultKey = "Left";
        </xsl:if>
        <xsl:variable name="specifier" select="//DynamicSpecifier[./ID eq $currentItem/SpecifierID]" />
        <xsl:choose>
          <xsl:when test="$specifier/@SpecifierType eq 'TrueFalse'" >
            KeyedDirInput = document.getElementById("<xsl:value-of select="concat('DynamicKey', $specifier/ID)" />");
            if (KeyedDirInput.value == "True")
            KeyedDir = DefaultKey;
            else if (DefaultKey == "Right")
            KeyedDir = "Left";
            else
            KeyedDir = "Right";
            <xsl:choose>
              <xsl:when test="(//Is7Block eq 'True') and (//RandomizationType eq 'SetNumberOfPresentations')" >
                <xsl:if test="$currentItem/OriginatingBlock eq '1'" >
                  Items1.push(new IATItem(<xsl:value-of select="concat(xs:string(position() + $startPosition), ', ', 'DisplayItem', ./StimulusDisplayID, ', ', ./ItemNum, ',  KeyedDir')" />));
                </xsl:if>
                <xsl:if test="$currentItem/OriginatingBlock eq '2'" >
                  Items2.push(new IATItem(<xsl:value-of select="concat(xs:string(position() + $startPosition), ', ', 'DisplayItem', ./StimulusDisplayID, ', ', ./ItemNum, ',  KeyedDir')" />));
                </xsl:if>
              </xsl:when>
              <xsl:otherwise>
                Items.push(new IATItem(<xsl:value-of select="concat(xs:string(position() + $startPosition), ', ', 'DisplayItem', ./StimulusDisplayID, ', ', ./ItemNum, ',  KeyedDir')" />));
              </xsl:otherwise>
            </xsl:choose>
          </xsl:when>
          <xsl:when test="$specifier/@SpecifierType eq 'Range'" >
            KeyedDirInput = document.getElementById("<xsl:value-of select="concat('DynamicKey', $specifier/ID)" />");
            if (KeyedDirInput != "Exclude")
            {
            if (KeyedDirInput.Value == "True")
            KeyedDir = DefaultKey;
            else if (DefaultKey == "Right")
            KeyedDir = "Left";
            else
            KeyedDir = "Right";
            <xsl:choose>
              <xsl:when test="(//Is7Block eq 'True') and (//RandomizationType eq 'SetNumberOfPresentations')" >
                <xsl:if test="$currentItem/OriginatingBlock eq '1'" >
                  Items1.push(new IATItem(<xsl:value-of select="concat(xs:string(position() + $startPosition), ', ', 'DisplayItem', ./StimulusDisplayID, ', ', ./ItemNum, ', ', KeyedDir, ')')" />);
                </xsl:if>
                <xsl:if test="$currentItem/OriginatingBlock eq '2'" >
                  Items2.push(new IATItem(<xsl:value-of select="concat(xs:string(position() + $startPosition), ', ', 'DisplayItem', ./StimulusDisplayID, ', ', ./ItemNum, ', ',  KeyedDir, ')')" />);
                </xsl:if>
              </xsl:when>
              <xsl:otherwise>
                Items.push(new IATItem(<xsl:value-of select="concat(xs:string(position() + $startPosition), ', ', 'DisplayItem', ./StimulusDisplayID, ', ', ./ItemNum, ', ', KeyedDir, ')')" />);
              </xsl:otherwise>
            </xsl:choose>
            }
          </xsl:when>
          <xsl:when test="$specifier/@SpecifierType eq 'Mask'" >
            FreeItemIDs.push(<xsl:value-of select="xs:string(position() + $startPosition)" />);
          </xsl:when>
          <xsl:when test="$specifier/@SpecifierType eq 'Selection'" >
            FreeItemIDs.push(<xsl:value-of select="xs:string(position() + $startPosition)" />);
          </xsl:when>
        </xsl:choose>
      </xsl:if>
    </xsl:for-each>
    <xsl:for-each-group select="//DynamicSpecifiers[@SpecifierType eq 'Mask']" group-by="SurveyName" >
      <xsl:for-each-group select="." group-by="ItemNum" >
        <xsl:variable name="specifierList" select="." />
        <xsl:for-each-group select="//IATEvent[(some $id in $specifierList/ID satisfies $id eq SpecifierID) and (@EventType eq 'IATItem')]" group-by="SpecifierID" >
          <xsl:for-each select="current-group()" >
            MaskItemTrueArray = new Array();
            MaskItemFalseArray = new Array();
            <xsl:variable name="stimList" select="." />
            <xsl:if test="count($stimList) eq 1" >
              <xsl:variable name="specifier" select="//DynamicSpecifiers[ID eq $stimList/SpecifierID]" />
              <xsl:value-of select="concat('KeyedDirInput = document.getElementById(&quot;DynamicKey', ID, ').value;&#x0A;'" />
              <xsl:if test="$itemList/KeyedDir eq 'DynamicLeft'" >
                if (KeyedDirInput == &quot;True&quot;)
                KeyedDir = &quot;Left&quot;;
                else
                KeyedDir = &quot;Right&quot;;
              </xsl:if>
              <xsl:if test="$itemList/KeyedDir eq 'DynamicRight'">
                if (KeyedDirInput == &quot;True&quot;)
                KeyedDir = &quot;Right&quot;;
                else
                KeyedDir = &quot;Left&quot;;
              </xsl:if>
              if (KeyedDirInput == "True")
              MaskItemTrueArray.push(new Array(<xsl:value-of select="concat($itemList/ItemNum, ', DisplayItem', $itemList/StimulusDisplayID, ', KeyedDir, ', $itemList/OriginatingBlock, ')')" />);
              else
              MaskItemFalseArray.push(new Array(<xsl:value-of select="concat($itemList/ItemNum, ', DisplayItem', $itemList/StimulusDisplayID, ', KeyedDir, ', $itemList/OriginatingBlock, ')')" />);
            </xsl:if>
            <xsl:if test="count($itemList) gt 1" >
              KeyedDirInput = document.getElementById("<xsl:value-of select="concat('DynamicKey', ID)" />").value;
              KeyedDirArray = new Array();
              OriginatingBlockArray = new Array();
              StimulusIDArray = new Array();
              ItemNumArray = new Array();
              <xsl:for-each select="$itemList">
                ItemNumArray.push(<xsl:value-of select="ItemNum" />);
                OriginatingBlockArray.push(<xsl:value-of select="OriginatingBlock" />);
                StimulusIDArray.push(<xsl:value-of select="concat('DisplayItem', StimulusDisplayID)" />);
                <xsl:if test="KeyedDir eq 'DynamicLeft'" >
                  if (KeyedDirInput == &quot;True&quot;)
                  KeyedDir = &quot;Left&quot;;
                  else
                  KeyedDir = &quot;Right&quot;;
                </xsl:if>
                <xsl:if test="./KeyedDir eq 'DynamicRight'">
                  if (KeyedDirInput == &quot;True&quot;)
                  KeyedDir = &quot;Right&quot;;
                  else
                  KeyedDir = &quot;Left&quot;;
                </xsl:if>
                KeyedDirArray.push(KeyedDir);
              </xsl:for-each>
              randomNum = Math.floor(Math.random() * KeyedDirArray.length);
              KeyedDir = KeyedDirArray[randomNum];
              if (KeyedDirInput == "True")
              MaskItemTrueArray.push(new Array(<xsl:value-of select="'ItemNumArray[randomNum], StimulusIDArray[randomNum], KeyedDir, OriginatingBlockArray[randomNum])'" />);
              else
              MaskItemFalseArray.push(new Array(<xsl:value-of select="'ItemNumArray[randomNum], StimulusIDArray[randomNum], KeyedDir, OriginatingBlockArray[randomNum])'" />);
            </xsl:if>
          </xsl:for-each>
          if (MaskItemTrueArray.length > MaskItemFalseArray.length)
          {
          for (ctr = 0; ctr &lt; MaskItemFalseArray.length; ctr++)
          randomNum = Math.floor(Math.random() * MaskItemTrueArray.length);


          <xsl:choose>
            <xsl:when test="(//Is7Block eq 'True') and (//RandomizationType eq 'SetNumberOfPresentations')" >
              if (MaskItemTrueArray[randomNum][3] == 1)
              Items1.push(new IATItem(FreeItemIDs.shift(), MaskItemTrueArray[randomNum][1], MaskItemTrueArray[randomNum][0], MaskItemTrueArray[randomNum][2]));
              else if (MaskItemTrueArray[randomNum][3] == 2)
              Items2.push(new IATItem(FreeItemIDs.shift(), MaskItemTrueArray[randomNum][1], MaskItemTrueArray[randomNum][0], MaskItemTrueArray[randomNum][2]));
            </xsl:when>
            <xsl:otherwise>
              Items.push(new IATItem(FreeItemIDs.shift(), MaskItemTrueArray[randomNum][1], MaskItemTrueArray[randomNum][0], MaskItemTrueArray[randomNum][2]));
            </xsl:otherwise>
          </xsl:choose>
          MaskItemTrueArray.splice(randomNum, 1);
          <xsl:choose>
            <xsl:when test="(//Is7Block eq 'True') and (//RandomizationType eq 'SetNumberOfPresentations')" >
              if (MaskItemFalseArray[ctr][3] == 1)
              Items1.push(new IATItem(FreeItemIDs.shift(), MaskItemFalseArray[ctr][1], MaskItemFalseArray[ctr][0], MaskItemFalseArray[ctr][2]));
              else if (MaskItemFalseArray[ctr][3] == 2)
              Items2.push(new IATItem(FreeItemIDs.shift(), MaskItemFalseArray[ctr][1], MaskItemFalseArray[ctr][0], MaskItemFalseArray[ctr][2]));
            </xsl:when>
            <xsl:otherwise>
              Items.push(new IATItem(FreeItemIDs.shift(), MaskItemFalseArray[ctr][1], MaskItemFalseArray[ctr][0], MaskItemFalseArray[ctr][2]));
            </xsl:otherwise>
          </xsl:choose>
          MaskItemFalseArray.splice(0, 1);
          }
          }
          else
          {
          for (ctr = 0; ctr &lt; MaskItemTrueArray.length; ctr++)
          {
          randomNum = Math.floor(Math.random() * MaskItemFalseArray.length);
          <xsl:choose>
            <xsl:when test="(//Is7Block eq 'True') and (//RandomizationType eq 'SetNumberOfPresentations')" >

              if (MaskItemFalseArray[randomNum][3] == 1)
              Items1.push(new IATItem(FreeItemIDs.shift(), MaskItemFalseArray[randomNum][1], MaskItemFalseArray[randomNum][0], MaskItemFalseArray[randomNum][2]));
              else if (MaskItemFalseArray[randomNum][3] == 2)
              Items2.push(new IATItem(FreeItemIDs.shift(), MaskItemFalseArray[randomNum][1], MaskItemFalseArray[randomNum][0], MaskItemFalseArray[randomNum][2]));
            </xsl:when>
            <xsl:otherwise>
              Items.push(new IATItem(FreeItemIDs.shift(), MaskItemFalseArray[randomNum][1], MaskItemFalseArray[randomNum][0], MaskItemFalseArray[randomNum][2]));
            </xsl:otherwise>
          </xsl:choose>
          MaskItemFalseArray.splice(randomNum, 1);
          <xsl:choose>
            <xsl:when test="(//Is7Block eq 'True') and (//RandomizationType eq 'SetNumberOfPresentations')" >
              if (MaskItemTrueArray[ctr][3] == 1)
              Items1.push(new IATItem(FreeItemIDs.shift(), MaskItemTrueArray[ctr][1], MaskItemTrueArray[ctr][0], MaskItemTrueArray[ctr][2]));
              else if (MaskItemTrueArray[ctr][3] == 2)
              Items2.push(new IATItem(FreeItemIDs.shift(), MaskItemTrueArray[ctr][1], MaskItemTrueArray[ctr][0], MaskItemTrueArray[ctr][2]));
            </xsl:when>
            <xsl:otherwise>
              Items.push(new IATItem(FreeItemIDs.shift(), MaskItemTrueArray[ctr][1], MaskItemTrueArray[ctr][0], MaskItemTrueArray[ctr][2]));
            </xsl:otherwise>
          </xsl:choose>
          MaskItemTrueArray.splice(0, 1);
          }
          }
        </xsl:for-each-group>
      </xsl:for-each-group>
    </xsl:for-each-group>
    <xsl:for-each select="//DynamicSpecifier[(some $i in $items satisfies ($i/SpecifierID eq ID) and (($i/OriginatingBlock eq '1') or ($i/OriginatingBlock eq '2'))) and (@SpecifierType eq 'Selection')]" >
      <xsl:variable name="specifier" select="." />
      <xsl:value-of select="concat('SelectedItem = document.getElementById(&quot;DynamicKey', ID, '&quot;).value;')" />
      SelectionSpecItemAry = new Array();
      <xsl:for-each-group select="$items[(@EventType eq 'IATItem') and (some $i in SpecifierID satisfies $i eq $specifier/ID)]" group-by="SpecifierArg" >
        SelectionSpecItemAry.push(new Array());
        ndx1 = SelectionSpecItemAry.length - 1;
        <xsl:for-each select="current-group()" >
          SelectionSpecItemAry[ndx1].push(new Array());
          ndx2 = SelectionSpecItemAry[ndx1].length - 1;
          <xsl:variable name="selectionItem" select="." />
          <xsl:value-of select="concat('SelectionSpecItemAry[ndx1][ndx2].push(', $selectionItem/SpecifierArg, ');&#x0A;')" />
          <xsl:value-of select="concat('SelectionSpecItemAry[ndx1][ndx2].push(&quot;', KeyedDir, '&quot;);&#x0A;')" />
          <xsl:value-of select="concat('SelectionSpecItemAry[ndx1][ndx2].push(DisplayItem', StimulusDisplayID, ');&#x0A;')" />
          <xsl:value-of select="concat('SelectionSpecItemAry[ndx1][ndx2].push(', $selectionItem/ItemNum, ');&#x0A;')" />
          <xsl:value-of select="concat('SelectionSpecItemAry[ndx1][ndx2].push(', $selectionItem/OriginatingBlock, ');&#x0A;')" />
        </xsl:for-each>
      </xsl:for-each-group>

      SelectionSpecNdx = 0;
      while (SelectedItem != SelectionSpecItemAry[SelectionSpecNdx][0][0])
      SelectionSpecNdx++;
      randomNum = Math.floor(Math.random() * SelectionSpecItemAry.length);
      if (SelectionSpecItemAry[SelectionSpecNdx][randomNum] == "DynamicRight")
      KeyedDir = "Right";
      if (SelectionSpecItemAry[SelectionSpecNdx][randomNum] == "DynamicLeft")
      KeyedDir = "Left";
      <xsl:choose>
        <xsl:when test="(//Is7Block eq 'True') and (//RandomizationType eq 'SetNumberOfPresentations')">
          if (SelectionOriginatingBlock  == 1)
          Items1.push(new IATItem(FreeItemIDs.shift(), SelectionSpecItemAry[SelectionSpecNdx][randomNum][2], SelectionSpecItemAry[SelectionSpecNdx][randomNum][3], KeyedDir));
          else if (SelectionOriginatingBlock == 2)
          Items2.push(new IATItem(FreeItemIDs.shift(), SelectionSpecItemAry[SelectionSpecNdx][randomNum][2], SelectionSpecItemAry[SelectionSpecNdx][randomNum][3], KeyedDir));
        </xsl:when>
        <xsl:otherwise>
          Items.push(new IATItem(FreeItemIDs.shift(), SelectionSpecItemAry[SelectionSpecNdx][randomNum][2], SelectionSpecItemAry[SelectionSpecNdx][randomNum][3], KeyedDir));
        </xsl:otherwise>
      </xsl:choose>
      RandomItem = SelectionSpecNdx;
      while (RandomItem == SelectionSpecNdx)
      RandomItem = Math.floor(Math.random() * SelectionSpecItemAry.length);
      randomNum = Math.floor(Math.random() * SelectionSpecItemAry[RandomItem].length);
      if (SelectionSpecItemAry[SelectionSpecNdx][randomNum] == "DynamicRight")
      KeyedDir = "Left";
      if (SelectionSpecItemAry[SelectionSpecNdx][randomNum] == "DynamicLeft")
      KeyedDir = "Right";
      <xsl:choose>
        <xsl:when test="(//Is7Block eq 'True') and (//RandomizationType eq 'SetNumberOfPresentations')">
          if (SelectionOriginatingBlock  == 1)
          Items1.push(new IATItem(FreeItemIDs.shift(), SelectionSpecItemAry[RandomItem][randomNum][2], SelectionSpecItemAry[RandomItem][randomNum][3], KeyedDir));
          else if (SelectionOriginatingBlock == 2)
          Items2.push(new IATItem(FreeItemIDs.shift(), SelectionSpecItemAry[RandomItem][randomNum][2], SelectionSpecItemAry[RandomItem][randomNum][3], KeyedDir));
        </xsl:when>
        <xsl:otherwise>
          Items.push(new IATItem(FreeItemIDs.shift(), SelectionSpecItemAry[RandomItem][randomNum][2], SelectionSpecItemAry[RandomItem][randomNum][3], KeyedDir));
        </xsl:otherwise>
      </xsl:choose>
    </xsl:for-each>



    <xsl:choose>
      <xsl:when test="(//Is7Block eq 'True') and (//RandomizationType eq 'SetNumberOfPresentations')" >



        if (Items2.length == 0)

        {
        for (ctr = 0; ctr &lt; Items1.length; ctr++)
        {
        ItemBlocks[ItemBlocks.length - 1].Items.push(Items1[ctr]);
        }

        }

        else if (Items1.length == 0)
        {
        for (ctr = 0; ctr &lt; Items2.length; ctr++)
        {
        ItemBlocks[ItemBlocks.length - 1].Items.push(Items2[ctr]);
        }

        }

        ctr = 0;

        while ((ctr &lt; Items1.length) &amp;&amp; (ctr &lt; Items2.length))
        {
        if (Items1.length &gt; Items2.length)
        {
        randomNum = Math.floor(Math.random() * Items1.length);
        ItemBlocks[ItemBlocks.length - 1].Items.push(Items1[randomNum]);
        Items1.splice(randomNum, 1);
        ItemBlocks[ItemBlocks.length - 1].Items.push(Items2[0]);
        Items2.splice(0, 1);
        }

        else

        {


        randomNum = Math.floor(Math.random() * Items2.length);


        ItemBlocks[ItemBlocks.length - 1].Items.push(Items1[0]);
        Items1.splice(0, 1);
        ItemBlocks[ItemBlocks.length - 1].Items.push(Items2[randomNum]);
        Items2.splice(randomNum, 1);
        }

        }


      </xsl:when>
      <xsl:otherwise>
        for (ctr = 0; ctr &lt; Items.length; ctr++)
        {
        ItemBlocks[ItemBlocks.length - 1].Items.push(Items[ctr]);
        }
      </xsl:otherwise>
    </xsl:choose>

  </xsl:template>
</xsl:stylesheet>