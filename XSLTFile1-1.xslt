<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xs="http://www.w3.org/2001/XMLSchema"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                version="2.0"
                exclude-result-prefixes="xs">

  <xsl:output method="xml" encoding="utf-8" indent="yes" cdata-section-elements="Function CodeLine" />

  <xsl:variable name="VariableDeclarations">
    <Declarations>
      <Declaration>var inPracticeBlock = false;</Declaration>
      <Declaration>var currentItemKeyedDir = "Left";</Declaration>
      <Declaration>var currentContinueKeyCode = 32;</Declaration>
      <Declaration>var isErrorMarked = false;</Declaration>
      <Declaration>var currentHandler = null;</Declaration>
      <Declaration>var currentStimulus = null;</Declaration>
      <Declaration>var currentItemNum = 0;</Declaration>
      <Declaration>var currentItemID;</Declaration>
      <Declaration>var EventList = new Array();</Declaration>
      <Declaration>var EventCtr = 0;</Declaration>
      <Declaration>var ErrorMark;</Declaration>
      <Declaration>var ImageLoadStatusTextElement;</Declaration>
      <Declaration>var ClickToStartElement;</Declaration>
      <Declaration>var ClickToStartText;</Declaration>
      <Declaration>var KeyedDirArray;</Declaration>
      <Declaration>var OriginatingBlockArray;</Declaration>
      <Declaration>var StimulusIDArray;</Declaration>
      <Declaration>var ItemNumArray;</Declaration>
      <Declaration>var KeyedDirInput;</Declaration>
      <Declaration>var Display;</Declaration>
      <Declaration>var DefaultKey;</Declaration>
      <Declaration>var FreeItemIDs;</Declaration>
      <Declaration>var Items;</Declaration>
      <Declaration>var Items1;</Declaration>
      <Declaration>var Items2;</Declaration>
      <xsl:for-each select="//DisplayItemList/IATDisplayItem">
        <Declaration>
          <xsl:value-of select="concat('var DI', ./ID, ';')"/>
        </Declaration>
      </xsl:for-each>
      <Declaration>var instructionBlock;</Declaration>
      <Declaration>var ItemBlocks;</Declaration>
      <Declaration>var InstructionBlocks;</Declaration>
      <Declaration>var alternate;</Declaration>
      <Declaration>var itemBlockCtr;</Declaration>
      <Declaration>var instructionsBlockCtr;</Declaration>
      <Declaration>var numAlternatedItemBlocks;</Declaration>
      <Declaration>var numAlternatedInstructionBlocks;</Declaration>
      <Declaration>var processIATItemFunctions = new Array();</Declaration>
      <Declaration>var itemCtr = 0;</Declaration>
    </Declarations>
  </xsl:variable>

  <xsl:variable name="GlobalAbbreviations">
    <xsl:variable name="Globals" select="string-join(for $elem in $VariableDeclarations/Declarations/Declaration return replace($elem, '^var\s+(.+);$', '$1'), ', ')" />
    <xsl:analyze-string select="$Globals" regex="([A-Za-z_][A-Za-z0-9_]*)(\s*=(\s+|[^;=/,&#x22;]+?|&#x22;[^&#x22;\n\r]*?&#x22;|\(([^;=,&#x22;]*?,?(&#x22;[^\n\r&#x22;]*?&#x22;)?)+\)|/[^/\n]+?/)+?)?,\s+">
      <xsl:matching-substring>
        <xsl:element name="Entry">
          <xsl:attribute name="type" select="'global'" />
          <xsl:element name="OrigName">
            <xsl:value-of select="regex-group(1)" />
          </xsl:element>
          <xsl:element name="NewName">
            <xsl:value-of select="concat('_g', position())" />
          </xsl:element>
          <xsl:element name="Assign">
            <xsl:value-of select="regex-group(2)" />
          </xsl:element>
        </xsl:element>
      </xsl:matching-substring>
    </xsl:analyze-string>
  </xsl:variable>

  <xsl:variable name="Classes">
    <xsl:element name="Class">
      <xsl:attribute name="ClassName" select="'IATDI'"/>
      <xsl:element name="Constructor">
        <xsl:attribute name="FunctionName" select="'IATDI'" />
        <xsl:element name="Params">
          <xsl:element name="Param">id</xsl:element>
          <xsl:element name="Param">x</xsl:element>
          <xsl:element name="Param">y</xsl:element>
          <xsl:element name="Param">width</xsl:element>
          <xsl:element name="Param">height</xsl:element>
        </xsl:element>
        <xsl:variable name="constructorBodyElems">
          <xsl:element name="Code">this.id = id;</xsl:element>
          <xsl:element name="Code">this.x = x;</xsl:element>
          <xsl:element name="Code">this.y = y;</xsl:element>
          <xsl:element name="Code">this.width = width;</xsl:element>
          <xsl:element name="Code">this.height = height;</xsl:element>
          <xsl:element name="Code">this.img = imgAry[id];</xsl:element>
          <xsl:element name="Code">this.imgTag = document.createElement("img");</xsl:element>
          <xsl:element name="Code">this.imgTag.appendChild(this.img);</xsl:element>
          <xsl:element name="Code">this.imgTag.id = "IATDI" + id.toString();</xsl:element>
        </xsl:variable>
        <xsl:element name="FunctionBody">
          <xsl:for-each select="$constructorBodyElems/Code">
            <xsl:element name="Code">
              <xsl:attribute name="LineNum" select="position"/>
              <xsl:value-of select="."/>
            </xsl:element>
          </xsl:for-each>
        </xsl:element>
      </xsl:element>

      <xsl:element name="PrototypeChain">
        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'SetImage'" />
          <xsl:element name="Params">
            <xsl:element name="Param">srcImg</xsl:element>
          </xsl:element>
          <xsl:variable name="functionBodyElems">
            <xsl:element name="Code">this.img = srcImg;</xsl:element>
          </xsl:variable>
          <xsl:element name="FunctionBody">
            <xsl:for-each select="$functionBodyElems/Code">
              <xsl:element name="Code">
                <xsl:attribute name="LineNum" select="'position()'" />
                <xsl:value-of select="." />
              </xsl:element>
            </xsl:for-each>
          </xsl:element>
        </xsl:element>

        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'Outline'"/>
          <xsl:element name="Params"/>
          <xsl:variable name="functionBodyElems">
            <xsl:element name="Code">this.imgTag.className = "outlinedDI";</xsl:element>
          </xsl:variable>
          <xsl:element name="FunctionBody">
            <xsl:for-each select="$functionBodyElems/Code">
              <xsl:element name="Code">
                <xsl:attribute name="LineNum" select="position()"/>
                <xsl:value-of select="."/>
              </xsl:element>
            </xsl:for-each>
          </xsl:element>
        </xsl:element>

        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'Display'"/>
          <xsl:element name="Params">
            <xsl:element name="Param">parentNode</xsl:element>
          </xsl:element>
          <xsl:variable name="functionBodyElems">
            <xsl:element name="Code">this.imgTag.src = this.src;</xsl:element>
            <xsl:element name="Code">parentNode.appendChild(this.imgTag);</xsl:element>
          </xsl:variable>
          <xsl:element name="FunctionBody">
            <xsl:for-each select="$functionBodyElems/Code">
              <xsl:element name="Code">
                <xsl:attribute name="LineNum" select="position()"/>
                <xsl:value-of select="."/>
              </xsl:element>
            </xsl:for-each>
          </xsl:element>
        </xsl:element>

        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'Hide'"/>
          <xsl:element name="Params"/>
          <xsl:variable name="functionBodyElems">
            <xsl:element name="Code">if (this.imgTag.parentNode) {</xsl:element>
            <xsl:element name="Code">this.imgTag.parentNode.removeChild(this.imgTag);</xsl:element>
            <xsl:element name="Code">}</xsl:element>
            <xsl:element name="Code">this.imgTag.className = "";</xsl:element>
          </xsl:variable>
          <xsl:element name="FunctionBody">
            <xsl:for-each select="$functionBodyElems/Code">
              <xsl:element name="Code">
                <xsl:attribute name="LineNum" select="position()"/>
                <xsl:value-of select="."/>
              </xsl:element>
            </xsl:for-each>
          </xsl:element>
        </xsl:element>
      </xsl:element>
    </xsl:element>


    <xsl:element name="Class">
      <xsl:attribute name="ClassName" select="'IATDisplay'"/>
      <xsl:element name="Constructor">
        <xsl:attribute name="FunctionName" select="'IATDisplay'" />
        <xsl:element name="Params"/>
        <xsl:variable name="constructorBodyElems">
          <xsl:element name="Code">
            <xsl:value-of select="concat('this.interiorWidth = ', //IATLayout/InteriorWidth, ';')"/>
          </xsl:element>
          <xsl:element name="Code">
            <xsl:value-of select="concat('this.interiorHeight = ', //IATLayout/InteriorHeight, ';')"/>
          </xsl:element>
          <xsl:element name="Code">
            <xsl:value-of select="concat('this.leftResponseKeyCodeUpper = ', //LeftResponseASCIIKeyCodeUpper, ';')"/>
          </xsl:element>
          <xsl:element name="Code">
            <xsl:value-of select="concat('this.rightResponseKeyCodeUpper = ', //RightResponseASCIIKeyCodeUpper, ';')"/>
          </xsl:element>
          <xsl:element name="Code">
            <xsl:value-of select="concat('this.leftResponseKeyCodeLower = ', //LeftResponseASCIIKeyCodeLower, ';')"/>
          </xsl:element>
          <xsl:element name="Code">
            <xsl:value-of select="concat('this.rightResponseKeyCodeLower = ', //RightResponseASCIIKeyCodeLower, ';')"/>
          </xsl:element>
          <xsl:element name="Code">this.divTag  = document.getElementById("IATDisplayDiv");</xsl:element>
          <xsl:element name="Code">this.displayItems = new Array();</xsl:element>
        </xsl:variable>
        <xsl:element name="FunctionBody">
          <xsl:for-each select="$constructorBodyElems/Code">
            <xsl:element name="Code">
              <xsl:attribute name="LineNum" select="position()"/>
              <xsl:value-of select="."/>
            </xsl:element>
          </xsl:for-each>
        </xsl:element>
      </xsl:element>

      <xsl:element name="PrototypeChain">
        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'AddDisplayItem'"/>
          <xsl:element name="Params">
            <xsl:element name="Param">di</xsl:element>
          </xsl:element>
          <xsl:variable name="functionBodyElems">
            <xsl:element name="Code">this.displayItems[this.displayItems.length] = di;</xsl:element>
            <xsl:element name="Code">di.Display(this.divTag);</xsl:element>
          </xsl:variable>
          <xsl:element name="FunctionBody">
            <xsl:for-each select="$functionBodyElems/Code">
              <xsl:element name="Code">
                <xsl:attribute name="LineNum" select="position()"/>
                <xsl:value-of select="."/>
              </xsl:element>
            </xsl:for-each>
          </xsl:element>
        </xsl:element>

        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'RemoveDisplayItem'"/>
          <xsl:element name="Params">
            <xsl:element name="Param">di</xsl:element>
          </xsl:element>
          <xsl:variable name="functionBodyElems">
            <xsl:element name="Code">for (ctr = 0; ctr &lt; this.displayItems.length; ctr++) {</xsl:element>
            <xsl:element name="Code">if (this.displayItems[ctr].id == di.id) {</xsl:element>
            <xsl:element name="Code">this.displayItems[ctr].Hide();</xsl:element>
            <xsl:element name="Code">this.displayItems.splice(ctr, 1);</xsl:element>
            <xsl:element name="Code">}</xsl:element>
            <xsl:element name="Code">}</xsl:element>
          </xsl:variable>
          <xsl:element name="FunctionBody">
            <xsl:for-each select="$functionBodyElems/Code">
              <xsl:element name="Code">
                <xsl:attribute name="LineNum" select="position()"/>
                <xsl:value-of select="."/>
              </xsl:element>
            </xsl:for-each>
          </xsl:element>
        </xsl:element>

        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'Clear'"/>
          <xsl:element name="Params"/>
          <xsl:variable name="functionBodyElems">
            <xsl:element name="Code">for (var ctr = 0; ctr &lt; this.displayItems.length; ctr++)</xsl:element>
            <xsl:element name="Code">this.displayItems[ctr].Hide();</xsl:element>
            <xsl:element name="Code">this.displayItems.splice(0, this.displayItems.length);</xsl:element>
          </xsl:variable>
          <xsl:element name="FunctionBody">
            <xsl:for-each select="$functionBodyElems/Code">
              <xsl:element name="Code">
                <xsl:attribute name="LineNum" select="position()"/>
                <xsl:value-of select="."/>
              </xsl:element>
            </xsl:for-each>
          </xsl:element>
        </xsl:element>

        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'StartTimer'"/>
          <xsl:element name="Params"/>
          <xsl:variable name="functionBodyElems">
            <xsl:element name="Code">this.startTime = (new Date()).getTime();</xsl:element>
          </xsl:variable>
          <xsl:element name="FunctionBody">
            <xsl:for-each select="$functionBodyElems/Code">
              <xsl:element name="Code">
                <xsl:attribute name="LineNum" select="position()"/>
                <xsl:value-of select="."/>
              </xsl:element>
            </xsl:for-each>
          </xsl:element>
        </xsl:element>

        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'GetDivTag'" />
          <xsl:element name="Params" />
          <xsl:variable name="functionBodyElems">
            <xsl:element name="Code">return this.divTag;</xsl:element>
          </xsl:variable>
          <xsl:element name="FunctionBody">
            <xsl:for-each select="$functionBodyElems/Code">
              <xsl:element name="Code">
                <xsl:attribute name="LineNum" select="position()"/>
                <xsl:value-of select="."/>
              </xsl:element>
            </xsl:for-each>
          </xsl:element>
        </xsl:element>

        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'IsLeftResponse'" />
          <xsl:element name="Params">
            <xsl:element name="Param">keyCode</xsl:element>
          </xsl:element>
          <xsl:variable name="functionBodyElems">
            <xsl:element name="Code">return ((keyCode == this.leftResponseKeyCodeUpper) || (keyCode == this.leftResponseKeyCodeLower));</xsl:element>
          </xsl:variable>
          <xsl:element name="FunctionBody">
            <xsl:for-each select="$functionBodyElems/Code">
              <xsl:element name="Code">
                <xsl:attribute name="LineNum" select="position()"/>
                <xsl:value-of select="."/>
              </xsl:element>
            </xsl:for-each>
          </xsl:element>
        </xsl:element>

        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'IsRightResponse'" />
          <xsl:element name="Params">
            <xsl:element name="Param">keyCode</xsl:element>
          </xsl:element>
          <xsl:variable name="functionBodyElems">
            <xsl:element name="Code">return ((keyCode == this.rightResponseKeyCodeUpper) || (keyCode == this.rightResponseKeyCodeLower));</xsl:element>
          </xsl:variable>
          <xsl:element name="FunctionBody">
            <xsl:for-each select="$functionBodyElems/Code">
              <xsl:element name="Code">
                <xsl:attribute name="LineNum" select="position()"/>
                <xsl:value-of select="."/>
              </xsl:element>
            </xsl:for-each>
          </xsl:element>
        </xsl:element>
      </xsl:element>

      <xsl:element name="Function">
        <xsl:attribute name="FunctionName" select="'GetStartTime'" />
        <xsl:element name="Params">
          <xsl:variable name="functionBodyElems">
            <xsl:element name="Code">return this.startTime;</xsl:element>
          </xsl:variable>
          <xsl:element name="FunctionBody">
            <xsl:for-each select="$functionBodyElems/Code">
              <xsl:element name="Code">
                <xsl:attribute name="LineNum" select="position()"/>
                <xsl:value-of select="."/>
              </xsl:element>
            </xsl:for-each>
          </xsl:element>
        </xsl:element>
      </xsl:element>
    </xsl:element>

    <xsl:element name="Class">
      <xsl:attribute name="ClassName" select="'IATEvent'"/>
      <xsl:element name="Constructor">
        <xsl:attribute name="FunctiomName" select="'IATEvent'" />
        <xsl:element name="Params">
          <xsl:element name="Param">handler</xsl:element>
        </xsl:element>
        <xsl:variable name="constructorBodyElems">
          <xsl:element name="Code">this.handler = handler;</xsl:element>
        </xsl:variable>
        <xsl:element name="FunctionBody">
          <xsl:for-each select="$constructorBodyElems/Code">
            <xsl:element name="Code">
              <xsl:attribute name="LineNum" select="position()"/>
              <xsl:value-of select="."/>
            </xsl:element>
          </xsl:for-each>
        </xsl:element>
      </xsl:element>

      <xsl:element name="PrototypeChain">
        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'Execute'" />
          <xsl:element name="Params"/>
          <xsl:variable name="functionBodyElems">
            <xsl:element name="Code">if (this.handler == null)</xsl:element>
            <xsl:element name="Code">EventList[++EventCtr].Execute();</xsl:element>
            <xsl:element name="Code">else {</xsl:element>
            <xsl:element name="Code">currentHandler = this.handler;</xsl:element>
            <xsl:element name="Code">EventUtil.addHandler(Display.GetDivTag(), "keypress", this.handler);</xsl:element>
            <xsl:element name="Code">}</xsl:element>
          </xsl:variable>
          <xsl:element name="FunctionBody">
            <xsl:for-each select="$functionBodyElems/Code">
              <xsl:element name="Code">
                <xsl:attribute name="LineNum" select="position()"/>
                <xsl:value-of select="."/>
              </xsl:element>
            </xsl:for-each>
          </xsl:element>
        </xsl:element>
      </xsl:element>
    </xsl:element>

    <xsl:element name="Class">
      <xsl:attribute name="ClassName" select="'IATSubmitEvent'"/>
      <xsl:element name="Constructor">
        <xsl:attribute name="FunctionName" select="'IATSubmitEvent'" />
        <xsl:element name="Params" />
        <xsl:variable name="constructorBodyElems">
          <xsl:element name="Code">IATEvent.call(this, null);</xsl:element>
        </xsl:variable>
        <xsl:element name="FunctionBody">
          <xsl:for-each select="$constructorBodyElems">
            <xsl:element name="Code">
              <xsl:attribute name="LineNum" select="position()"/>
              <xsl:value-of select="."/>
            </xsl:element>
          </xsl:for-each>
        </xsl:element>
      </xsl:element>

      <xsl:element name="PrototypeChain">
        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'Execute'"/>
          <xsl:element name="Params"/>
          <xsl:variable name="functionBodyElems">
            <xsl:element name="Code">var numItemsInput = document.createElement("input");</xsl:element>
            <xsl:element name="Code">numItemsInput.name = "NumItems";</xsl:element>
            <xsl:element name="Code">numItemsInput.type = "hidden";</xsl:element>
            <xsl:element name="Code">numItemsInput.value = currentItemNum.toString();</xsl:element>
            <xsl:element name="Code">Display.GetDivTag().appendChild(numItemsInput);</xsl:element>
            <xsl:element name="Code">var form = document.getElementById("IATForm");</xsl:element>
            <xsl:element name="Code">form.submit();</xsl:element>
          </xsl:variable>
          <xsl:element name="FunctionBody">
            <xsl:for-each select="$functionBodyElems/Code">
              <xsl:element name="Code">
                <xsl:attribute name="LineNum" select="position()"/>
                <xsl:value-of select="."/>
              </xsl:element>
            </xsl:for-each>
          </xsl:element>
        </xsl:element>
      </xsl:element>
    </xsl:element>

    <xsl:element name="Class">
      <xsl:attribute name="ClassName" select="'IATItem'"/>
      <xsl:element name="Constructor">
        <xsl:attribute name="FunctionName" select="'IATItem'" />
        <xsl:element name="Params">
          <xsl:element name="Param">stimulus</xsl:element>
          <xsl:element name="Param">itemNum</xsl:element>
          <xsl:element name="Param">keyedDir</xsl:element>
        </xsl:element>
        <xsl:variable name="constructorBodyElems">
          <xsl:element name="Code">this.keyedDir = keyedDir;</xsl:element>
          <xsl:element name="Code">if (keyedDir == "Left") </xsl:element>
          <xsl:element name="Code">IATEvent.call(this, OnLeftKeyedItem);</xsl:element>
          <xsl:element name="Code">else</xsl:element>
          <xsl:element name="Code">IATEvent.call(this, OnRightKeyedItem);</xsl:element>
          <xsl:element name="Code">this.isErrorMarked = false;</xsl:element>
          <xsl:element name="Code">this.stimulus = stimulus;</xsl:element>
          <xsl:element name="Code">this.itemNum = itemNum;</xsl:element>
        </xsl:variable>
        <xsl:element name="FunctionBody">
          <xsl:for-each select="$constructorBodyElems/Code">
            <xsl:element name="Code">
              <xsl:attribute name="LineNum" select="position()"/>
              <xsl:value-of select="."/>
            </xsl:element>
          </xsl:for-each>
        </xsl:element>
      </xsl:element>

      <xsl:element name="PrototypeChain">
        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'Execute'"/>
          <xsl:element name="Params"/>
          <xsl:variable name="functionBodyElems">
            <xsl:element name="Code">currentItemKeyedDir = this.keyedDir;</xsl:element>
            <xsl:element name="Code">currentStimulus = this.stimulus;</xsl:element>
            <xsl:element name="Code">currentItemNum = this.itemNum;</xsl:element>
            <xsl:element name="Code">Display.AddDisplayItem(this.stimulus);</xsl:element>
            <xsl:element name="Code">Display.StartTimer();</xsl:element>
            <xsl:element name="Code">window.setTimeout(IATEvent.prototype.Execute.call(this), 100);</xsl:element>
          </xsl:variable>
          <xsl:element name="FunctionBody">
            <xsl:for-each select="$functionBodyElems/Code">
              <xsl:element name="Code">
                <xsl:attribute name="LineNum" select="position()"/>
                <xsl:value-of select="."/>
              </xsl:element>
            </xsl:for-each>
          </xsl:element>
        </xsl:element>
      </xsl:element>
    </xsl:element>

    <xsl:element name="Class">
      <xsl:attribute name="ClassName" select="'IATBeginBlock'"/>
      <xsl:element name="Constructor">
        <xsl:attribute name="FunctionName" select="'IATBeginBlock'" />
        <xsl:element name="Params">
          <xsl:element name="Param">isPracticeBlock</xsl:element>
          <xsl:element name="Param">leftDisplayItem</xsl:element>
          <xsl:element name="Param">rightDisplayItem</xsl:element>
          <xsl:element name="Param">instructionsDisplayItem</xsl:element>
        </xsl:element>
        <xsl:variable name="constructorBodyElems">
          <xsl:element name="Code">IATEvent.call(this, null);</xsl:element>
          <xsl:element name="Code">this.isPracticeBlock = isPracticeBlock;</xsl:element>
          <xsl:element name="Code">this.leftDisplayItem = leftDisplayItem;</xsl:element>
          <xsl:element name="Code">this.rightDisplayItem = rightDisplayItem;</xsl:element>
          <xsl:element name="Code">this.instructionsDisplayItem = instructionsDisplayItem;</xsl:element>
        </xsl:variable>
        <xsl:element name="FunctionBody">
          <xsl:for-each select="$constructorBodyElems/Code">
            <xsl:element name="Code">
              <xsl:attribute name="LineNum" select="position()"/>
              <xsl:value-of select="."/>
            </xsl:element>
          </xsl:for-each>
        </xsl:element>
      </xsl:element>

      <xsl:element name="PrototypeChain">
        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'Execute'"/>
          <xsl:element name="Params"/>
          <xsl:variable name="functionBodyElems">
            <xsl:element name="Code">inPracticeBlock = this.isPracticeBlock;</xsl:element>
            <xsl:element name="Code">Display.AddDisplayItem(this.leftDisplayItem);</xsl:element>
            <xsl:element name="Code">Display.AddDisplayItem(this.rightDisplayItem);</xsl:element>
            <xsl:element name="Code">Display.AddDisplayItem(this.instructionsDisplayItem);</xsl:element>
            <xsl:element name="Code">IATEvent.prototype.Execute.call(this);</xsl:element>
          </xsl:variable>
          <xsl:element name="FunctionBody">
            <xsl:for-each select="$functionBodyElems/Code">
              <xsl:element name="Code">
                <xsl:attribute name="LineNum" select="position()"/>
                <xsl:value-of select="."/>
              </xsl:element>
            </xsl:for-each>
          </xsl:element>
        </xsl:element>
      </xsl:element>
    </xsl:element>

    <xsl:element name="Class">
      <xsl:attribute name="ClassName" select="'IATEndBlock'"/>
      <xsl:element name="Constructor">
        <xsl:attribute name="FunctionName" select="'IATEndBlock'" />
        <xsl:element name="Params" />
        <xsl:variable name="constructorBodyElems">
          <xsl:element name="Code">IATEvent.call(this, null);</xsl:element>
        </xsl:variable>
        <xsl:element name="FunctionBody">
          <xsl:for-each select="$constructorBodyElems/Code">
            <xsl:element name="Code">
              <xsl:attribute name="LineNum" select="position()"/>
              <xsl:value-of select="."/>
            </xsl:element>
          </xsl:for-each>
        </xsl:element>
      </xsl:element>

      <xsl:element name="PrototypeChain">
        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'Execute'"/>
          <xsl:element name="Params"/>
          <xsl:variable name="functionBodyElems">
            <xsl:element name="Code">inPracticeBlock = false;</xsl:element>
            <xsl:element name="Code">Display.Clear();</xsl:element>
            <xsl:element name="Code">IATEvent.prototype.Execute.call(this);</xsl:element>
          </xsl:variable>
          <xsl:element name="FunctionBody">
            <xsl:for-each select="$functionBodyElems/Code">
              <xsl:element name="Code">
                <xsl:attribute name="LineNum" select="position()"/>
                <xsl:value-of select="."/>
              </xsl:element>
            </xsl:for-each>
          </xsl:element>
        </xsl:element>
      </xsl:element>
    </xsl:element>

    <xsl:element name="Class">
      <xsl:attribute name="ClassName" select="'IATInstructionScreen'"/>
      <xsl:element name="Constructor">
        <xsl:attribute name="FunctionBody" select="'IATInstructionScreen'" />
        <xsl:element name="Params">
          <xsl:element name="Param">continueChar</xsl:element>
          <xsl:element name="Param">continueInstructionsDI</xsl:element>
        </xsl:element>
        <xsl:variable name="constructorBodyElems">
          <xsl:element name="Code">IATEvent.call(this, function(event) {</xsl:element>
          <xsl:element name="Code">event = EventUtil.getEvent(event);</xsl:element>
          <xsl:element name="Code">var keyCode = EventUtil.getCharCode(event);</xsl:element>
          <xsl:element name="Code">if (keyCode == currentContinueKeyCode) {</xsl:element>
          <xsl:element name="Code">Display.Clear();</xsl:element>
          <xsl:element name="Code">EventUtil.removeHandler(Display.GetDivTag(), "keypress", currentHandler);</xsl:element>
          <xsl:element name="Code">IATEvent.prototype.Execute.call(this);</xsl:element>
          <xsl:element name="Code">}</xsl:element>
          <xsl:element name="Code">});</xsl:element>
          <xsl:element name="Code">this.continueChar = continueChar;</xsl:element>
          <xsl:element name="Code">this.continueInstructionsDI = continueInstructionsDI;</xsl:element>
        </xsl:variable>
        <xsl:element name="FunctionBody">
          <xsl:for-each select="$constructorBodyElems/Code">
            <xsl:element name="Code">
              <xsl:attribute name="LineNum" select="position()"/>
              <xsl:value-of select="."/>
            </xsl:element>
          </xsl:for-each>
        </xsl:element>
      </xsl:element>

      <xsl:element name="PrototypeChain">
        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'Execute'"/>
          <xsl:variable name="functionBodyElems">
            <xsl:element name="Code">Display.AddDisplayItem(this.continueInstructionsDI);</xsl:element>
            <xsl:element name="Code">currentContinueKeyCode = this.continueChar;</xsl:element>
            <xsl:element name="Code">IATEvent.prototype.Execute.call(this);</xsl:element>
          </xsl:variable>
          <xsl:element name="FunctionBody">
            <xsl:for-each select="$functionBodyElems/Code">
              <xsl:element name="Code">
                <xsl:attribute name="LineNum" select="position()"/>
                <xsl:value-of select="."/>
              </xsl:element>
            </xsl:for-each>
          </xsl:element>
        </xsl:element>
      </xsl:element>
    </xsl:element>

    <xsl:element name="Class">
      <xsl:attribute name="ClassName" select="'IATTextInstructionsScreen'"/>
      <xsl:element name="Constructor">
        <xsl:attribute name="FunctionName" select="'IATTextInstructionsScreen'" />
        <xsl:element name="Params">
          <xsl:element name="Param">continueChar</xsl:element>
          <xsl:element name="Param">continueInstructionsDI</xsl:element>
          <xsl:element name="Param">textInstructionsDI</xsl:element>
        </xsl:element>
        <xsl:variable name="constructorBodyElems">
          <xsl:element name="Code">IATInstructionScreen.call(this, continueChar, continueInstructionsDI);</xsl:element>
          <xsl:element name="Code">this.textInstructionsDI = textInstructionsDI;</xsl:element>
        </xsl:variable>
        <xsl:element name="FunctionBody">
          <xsl:for-each select="$constructorBodyElems/Code">
            <xsl:element name="Code">
              <xsl:attribute name="LineNum" select="position()"/>
              <xsl:value-of select="."/>
            </xsl:element>
          </xsl:for-each>
        </xsl:element>
      </xsl:element>

      <xsl:element name="PrototypeChain">
        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'Execute'"/>
          <xsl:element name="Params"/>
          <xsl:variable name="functionBodyElems">
            <xsl:element name="Code">Display.AddDisplayItem(this.textInstructionsDI);</xsl:element>
            <xsl:element name="Code">IATInstructionScreen.prototype.Execute.call(this);</xsl:element>
          </xsl:variable>
          <xsl:element name="FunctionBody">
            <xsl:for-each select="$functionBodyElems/Code">
              <xsl:element name="Code">
                <xsl:attribute name="LineNum" select="position()"/>
                <xsl:value-of select="."/>
              </xsl:element>
            </xsl:for-each>
          </xsl:element>
        </xsl:element>
      </xsl:element>
    </xsl:element>

    <xsl:element name="Class">
      <xsl:attribute name="ClassName" select="'IATMockItemInstructionScreen'"/>
      <xsl:element name="Constructor">
        <xsl:attribute name="FunctionName" select="'IATMockItemInstructionScreen'" />
        <xsl:element name="Params">
          <xsl:element name="Param">continueChar</xsl:element>
          <xsl:element name="Param">continueInstructionsDI</xsl:element>
          <xsl:element name="Param">leftResponseDI</xsl:element>
          <xsl:element name="Param">rightResponseDI</xsl:element>
          <xsl:element name="Param">stimulusDI</xsl:element>
          <xsl:element name="Param">instructionsDI</xsl:element>
          <xsl:element name="Param">errorMarked</xsl:element>
          <xsl:element name="Param">outlineLeftResponse</xsl:element>
          <xsl:element name="Param">outlineRightResponse</xsl:element>
        </xsl:element>
        <xsl:variable name="constructorBodyElems">
          <xsl:element name="Code">IATInstructionScreen.call(this, continueChar, continueInstructionsDI);</xsl:element>
          <xsl:element name="Code">this.leftResponseDI = leftResponseDI;</xsl:element>
          <xsl:element name="Code">this.rightResponseDI = rightResponseDI;</xsl:element>
          <xsl:element name="Code">this.stimulusDI = stimulusDI;</xsl:element>
          <xsl:element name="Code">this.instructionsDI = instructionsDI;</xsl:element>
          <xsl:element name="Code">this.errorMarked = errorMarked;</xsl:element>
          <xsl:element name="Code">this.outlineLeftResponse = outlineLeftResponse;</xsl:element>
          <xsl:element name="Code">this.outlineRightResponse = outlineRightResponse;</xsl:element>
        </xsl:variable>
        <xsl:element name="FunctionBody">
          <xsl:for-each select="$constructorBodyElems/Code">
            <xsl:element name="Code">
              <xsl:attribute name="LineNum" select="position()"/>
              <xsl:value-of select="."/>
            </xsl:element>
          </xsl:for-each>
        </xsl:element>
      </xsl:element>

      <xsl:element name="PrototypeChain">
        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'Execute'"/>
          <xsl:element name="Params"/>
          <xsl:variable name="functionBodyElems">
            <xsl:element name="Code">if (this.outlineLeftResponse)</xsl:element>
            <xsl:element name="Code">this.leftResponseDI.Outline();</xsl:element>
            <xsl:element name="Code">Display.AddDisplayItem(this.leftResponseDI);</xsl:element>
            <xsl:element name="Code">if (this.outlineRightResponse)</xsl:element>
            <xsl:element name="Code">this.rightResponseDI.Outline();</xsl:element>
            <xsl:element name="Code">Display.AddDisplayItem(this.rightResponseDI);</xsl:element>
            <xsl:element name="Code">Display.AddDisplayItem(this.stimulusDI);</xsl:element>
            <xsl:element name="Code">Display.AddDisplayItem(this.instructionsDI);</xsl:element>
            <xsl:element name="Code">if (this.errorMarked)</xsl:element>
            <xsl:element name="Code">Display.AddDisplayItem(ErrorMark);</xsl:element>
            <xsl:element name="Code">IATInstructionScreen.prototype.Execute.call(this);</xsl:element>
          </xsl:variable>
          <xsl:element name="FunctionBody">
            <xsl:for-each select="$functionBodyElems/Code">
              <xsl:element name="Code">
                <xsl:attribute name="LineNum" select="position()"/>
                <xsl:value-of select="."/>
              </xsl:element>
            </xsl:for-each>
          </xsl:element>
        </xsl:element>
      </xsl:element>
    </xsl:element>

    <xsl:element name="Class">
      <xsl:attribute name="ClassName" select="'IATKeyedInstructionScreen'"/>
      <xsl:element name="Constructor">
        <xsl:attribute name="FunctionName" select="'IATKeyedInstruction'" />
        <xsl:element name="Params">
          <xsl:element name="Param">continueChar</xsl:element>
          <xsl:element name="Param">continueInstructionsDI</xsl:element>
          <xsl:element name="Param">instructionsDI</xsl:element>
          <xsl:element name="Param">leftResponseDI</xsl:element>
          <xsl:element name="Param">rightResponseDI</xsl:element>
        </xsl:element>
        <xsl:variable name="constructorBodyElems">
          <xsl:element name="Code">IATInstructionScreen.call(this, continueChar, continueInstructionsDI);</xsl:element>
          <xsl:element name="Code">this.instructionsDI = instructionsDI;</xsl:element>
          <xsl:element name="Code">this.leftResponseDI = leftResponseDI;</xsl:element>
          <xsl:element name="Code">this.rightResponseDI = rightResponseDI;</xsl:element>
        </xsl:variable>
        <xsl:element name="FunctionBody">
          <xsl:for-each select="$constructorBodyElems/Code">
            <xsl:element name="Code">
              <xsl:attribute name="LineNum" select="position()"/>
              <xsl:value-of select="."/>
            </xsl:element>
          </xsl:for-each>
        </xsl:element>
      </xsl:element>

      <xsl:element name="PrototypeChain">
        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'Execute'"/>
          <xsl:element name="Params"/>
          <xsl:variable name="functionBodyElems">
            <xsl:element name="Code">Display.AddDisplayItem(this.instructionsDI);</xsl:element>
            <xsl:element name="Code">Display.AddDisplayItem(this.leftResponseDI);</xsl:element>
            <xsl:element name="Code">Display.AddDisplayItem(this.rightResponseDI);</xsl:element>
            <xsl:element name="Code">IATInstructionScreen.prototype.Executecall(this);</xsl:element>
          </xsl:variable>
          <xsl:element name="FunctionBody">
            <xsl:for-each select="$functionBodyElems/Code">
              <xsl:element name="Code">
                <xsl:attribute name="LineNum" select="position()"/>
                <xsl:value-of select="."/>
              </xsl:element>
            </xsl:for-each>
          </xsl:element>
        </xsl:element>
      </xsl:element>
    </xsl:element>

    <xsl:element name="Class">
      <xsl:attribute name="ClassName" select="'IATBlock'"/>
      <xsl:element name="Constructor">
        <xsl:attribute name="FunctionName" select="'IATBlock'" />
        <xsl:element name="Params">
          <xsl:element name="Param">blockNum</xsl:element>
          <xsl:element name="Param">blockPosition</xsl:element>
          <xsl:element name="Param">numPresentations</xsl:element>
          <xsl:element name="Param">alternatedWith</xsl:element>
        </xsl:element>
        <xsl:variable name="functionBodyElems">
          <xsl:element name="Code">this.blockNum = blockNum;</xsl:element>
          <xsl:element name="Code">this.blockPosition = blockPosition;</xsl:element>
          <xsl:element name="Code">this.numPresentations = numPresentations;</xsl:element>
          <xsl:element name="Code">this.alternatedWith = alternatedWith;</xsl:element>
          <xsl:element name="Code">this.BeginBlockEvent = null;</xsl:element>
          <xsl:element name="Code">this.EndBlockEvent = null;</xsl:element>
          <xsl:element name="Code">this.Items = new Array();</xsl:element>
        </xsl:variable>
        <xsl:element name="FunctionBody">
          <xsl:for-each select="$functionBodyElems/Code">
            <xsl:element name="Code">
              <xsl:attribute name="LineNum" select="position()"/>
              <xsl:value-of select="."/>
            </xsl:element>
          </xsl:for-each>
        </xsl:element>
      </xsl:element>

      <xsl:element name="PrototypeChain">
        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'AddItem'"/>
          <xsl:element name="Params">
            <xsl:element name="Param">item</xsl:element>
          </xsl:element>
          <xsl:variable name="functionBodyElems">
            <xsl:element name="Code">this.Items.push(item);</xsl:element>
          </xsl:variable>
          <xsl:element name="FunctionBody">
            <xsl:for-each select="$functionBodyElems/Code">
              <xsl:element name="Code">
                <xsl:attribute name="LineNum" select="position()"/>
                <xsl:value-of select="."/>
              </xsl:element>
            </xsl:for-each>
          </xsl:element>
        </xsl:element>

        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'GenerateContents'"/>
          <xsl:element name="Params">
            <xsl:element name="Param">randomization</xsl:element>
          </xsl:element>
          <xsl:variable name="functionBodyElems">
            <xsl:element name="Code">var result = new Array();</xsl:element>
            <xsl:element name="Code">result.push(this.BeginBlockEvent);</xsl:element>
            <xsl:element name="Code">var ctr;</xsl:element>
            <xsl:element name="Code">var currItemNdx, lastItemNdx = -1;</xsl:element>
            <xsl:element name="Code">if (randomization == "None") {</xsl:element>
            <xsl:element name="Code">for (ctr = 0; ctr &lt; Items.length; ctr++)</xsl:element>
            <xsl:element name="Code">result.push(this.Items[ctr]);</xsl:element>
            <xsl:element name="Code">} else if (randomization == "RandomOrder") {</xsl:element>
            <xsl:element name="Code">var tempItems = new Array();</xsl:element>
            <xsl:element name="Code">for (ctr = 0; ctr &lt; this.Items.length; ctr++)</xsl:element>
            <xsl:element name="Code">tempItems.push(this.Items[ctr]);</xsl:element>
            <xsl:element name="Code">for (ctr = 0; ctr &lt; this.Items.length; ctr++) {</xsl:element>
            <xsl:element name="Code">var ndx = Math.floor(Math.random() * tempItems.length);</xsl:element>
            <xsl:element name="Code">result.push(tempItems[ndx]);</xsl:element>
            <xsl:element name="Code">tempItems.splice(ndx, 1);</xsl:element>
            <xsl:element name="Code">}</xsl:element>
            <xsl:element name="Code">} else if (randomization == "SetNumberOfPresentations") {</xsl:element>
            <xsl:element name="Code">for (ctr = 0; ctr &lt; this.numPresentations; ctr++) {</xsl:element>
            <xsl:element name="Code">currItemNdx = Math.floor(Math.random() * this.Items.length);</xsl:element>
            <xsl:element name="Code">while (currItemNdx == lastItemNdx)</xsl:element>
            <xsl:element name="Code">currItemNdx = Math.floor(Math.random() * this.Items.length);</xsl:element>
            <xsl:element name="Code">result.push(this.Items[currItemNdx]);</xsl:element>
            <xsl:element name="Code">lastItemNdx = currItemNdx;</xsl:element>
            <xsl:element name="Code">}</xsl:element>
            <xsl:element name="Code">}</xsl:element>
            <xsl:element name="Code">result.push(this.EndBlockEvent);</xsl:element>
            <xsl:element name="Code">return result;</xsl:element>
          </xsl:variable>
          <xsl:element name="FunctionBody">
            <xsl:for-each select="$functionBodyElems/Code">
              <xsl:element name="Code">
                <xsl:attribute name="LineNum" select="position()"/>
                <xsl:value-of select="."/>
              </xsl:element>
            </xsl:for-each>
          </xsl:element>
        </xsl:element>
      </xsl:element>
    </xsl:element>

    <xsl:element name="Class">
      <xsl:attribute name="ClassName" select="'IATInstructionBlock'"/>
      <xsl:element name="Constructor">
        <xsl:attribute name="FunctionName" select="'IATInstructionBlock'" />
        <xsl:element name="Params">
          <xsl:element name="Param">alternatedWith</xsl:element>
          <xsl:element name="Param">blockPosition</xsl:element>
        </xsl:element>
        <xsl:variable name="constructorBodyElems">
          <xsl:element name="Code">this.alternatedWith = alternatedWith;</xsl:element>
          <xsl:element name="Code">this.blockPosition = blockPosition;</xsl:element>
          <xsl:element name="Code">this.screens = new Array();</xsl:element>
        </xsl:variable>
        <xsl:element name="FunctionBody">
          <xsl:for-each select="$constructorBodyElems/Code">
            <xsl:element name="Code">
              <xsl:attribute name="LineNum" select="position()"/>
              <xsl:value-of select="."/>
            </xsl:element>
          </xsl:for-each>
        </xsl:element>
      </xsl:element>

      <xsl:element name="PrototypeChain">
        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'AddScreen'"/>
          <xsl:element name="Params">
            <xsl:element name="Param">screenType</xsl:element>
            <xsl:element name="Param">ctorArgAry</xsl:element>
          </xsl:element>
          <xsl:variable name="functionBodyElems">
            <xsl:element name="Code">var screen;</xsl:element>
            <xsl:element name="Code">if (screenType == "Text")</xsl:element>
            <xsl:element name="Code">screen = new IATTextInstructionScreen(ctorArgAry);</xsl:element>
            <xsl:element name="Code">else if (screenType == "Keyed")</xsl:element>
            <xsl:element name="Code">screen = new IATKeyedInstructionScreen(ctorArgAry);</xsl:element>
            <xsl:element name="Code">else</xsl:element>
            <xsl:element name="Code">screen = new IATMockItemInstructionScreen(ctorArgAry);</xsl:element>
            <xsl:element name="Code">this.screens.push(screen);</xsl:element>
          </xsl:variable>
          <xsl:element name="FunctionBody">
            <xsl:for-each select="$functionBodyElems/Code">
              <xsl:element name="Code">
                <xsl:attribute name="LineNum" select="position()"/>
                <xsl:value-of select="."/>
              </xsl:element>
            </xsl:for-each>
          </xsl:element>
        </xsl:element>
      </xsl:element>
    </xsl:element>
  </xsl:variable>

  <xsl:variable name="processItemFunctions" >
    <xsl:call-template name="GenerateProcessItemFunctions" />
  </xsl:variable>

  <xsl:variable name="Functions">

    <xsl:element name="Function">
      <xsl:attribute name="FunctionName" select="'OnLeftKeyedItem'" />
      <xsl:element name="Params">
        <xsl:element name="Param">e</xsl:element>
      </xsl:element>
      <xsl:variable name="functionBodyElems">
        <xsl:element name="Code">var event = EventUtil.getEvent(e);</xsl:element>
        <xsl:element name="Code">var keyCode = EventUtil.getCharCode(event);</xsl:element>
        <xsl:element name="Code">if (Display.IsLeftResponse(keyCode)) {</xsl:element>
        <xsl:element name="Code">if (!inPracticeBlock) {</xsl:element>
        <xsl:element name="Code">var latencyItemName = document.createElement("input");</xsl:element>
        <xsl:element name="Code">latencyItemName.name = "ItemNum" + itemCtr.toString();</xsl:element>
        <xsl:element name="Code">latencyItemName.value = this.itemNum.toString();</xsl:element>
        <xsl:element name="Code">latencyItemName.type = "hidden";</xsl:element>
        <xsl:element name="Code">Display.GetDivTag().appendChild(latencyItemName);</xsl:element>
        <xsl:element name="Code">var latency = (new Date()).getTime() - Display.GetStartTime();</xsl:element>
        <xsl:element name="Code">var latencyOutput = document.createElement("input");</xsl:element>
        <xsl:element name="Code">latencyOutput.name = "ItemLatency" + itemCtr.toString();</xsl:element>
        <xsl:element name="Code">itemCtr++;</xsl:element>
        <xsl:element name="Code">latencyOutput.type = "hidden";</xsl:element>
        <xsl:element name="Code">latencyOutput.value = latency.toString();</xsl:element>
        <xsl:element name="Code">Display.GetDivTag().appendChild(latencyOutput);</xsl:element>
        <xsl:element name="Code">}</xsl:element>
        <xsl:element name="Code">if (isErrorMarked)</xsl:element>
        <xsl:element name="Code">Display.RemoveDisplayItem(ErrorMark);</xsl:element>
        <xsl:element name="Code">isErrorMarked = false;</xsl:element>
        <xsl:element name="Code">Display.RemoveDisplayItem(currentStimulus);</xsl:element>
        <xsl:element name="Code">EventUtil.removeHandler(Display.GetDivTag(), "keypress", currentHandler);</xsl:element>
        <xsl:element name="Code">EventList[++EventCtr].Execute();</xsl:element>
        <xsl:element name="Code">} else if (Display.IsRightResponse(keyCode)) {</xsl:element>
        <xsl:element name="Code">if (!isErrorMarked) {</xsl:element>
        <xsl:element name="Code">Display.AddDisplayItem(ErrorMark);</xsl:element>
        <xsl:element name="Code">isErrorMarked = true;</xsl:element>
        <xsl:element name="Code">}</xsl:element>
        <xsl:element name="Code">}</xsl:element>
      </xsl:variable>
      <xsl:element name="FunctionBody">
        <xsl:for-each select="$functionBodyElems/Code">
          <xsl:element name="Code">
            <xsl:attribute name="LineNum" select="position()"/>
            <xsl:value-of select="."/>
          </xsl:element>
        </xsl:for-each>
      </xsl:element>
    </xsl:element>

    <xsl:element name="Function">
      <xsl:attribute name="FunctionName" select="'OnRightKeyedItem'" />
      <xsl:element name="Params">
        <xsl:element name="Param">e</xsl:element>
      </xsl:element>
      <xsl:variable name="functionBodyElems">
        <xsl:element name="Code">var event = EventUtil.getEvent(event);</xsl:element>
        <xsl:element name="Code">var keyCode = EventUtil.getCharCode(event);</xsl:element>
        <xsl:element name="Code">if (Display.IsRightResponse(keyCode)) {</xsl:element>
        <xsl:element name="Code">if (!inPracticeBlock) {</xsl:element>
        <xsl:element name="Code">var latencyItemName = document.createElement("input");</xsl:element>
        <xsl:element name="Code">latencyItemName.name = "ItemNum" + itemCtr.toString();</xsl:element>
        <xsl:element name="Code">latencyItemName.value = this.itemNum.toString();</xsl:element>
        <xsl:element name="Code">latencyItemName.type = "hidden";</xsl:element>
        <xsl:element name="Code">Display.GetDivTag().appendChild(latencyItemName);</xsl:element>
        <xsl:element name="Code">var latency = (new Date()).getTime() - Display.GetStartTime();</xsl:element>
        <xsl:element name="Code">var latencyOutput = document.createElement("input");</xsl:element>
        <xsl:element name="Code">latencyOutput.name = "ItemLatency" + itemCtr.toString();</xsl:element>
        <xsl:element name="Code">itemCtr++;</xsl:element>
        <xsl:element name="Code">latencyOutput.type = "hidden";</xsl:element>
        <xsl:element name="Code">latencyOutput.value = latency.toString();</xsl:element>
        <xsl:element name="Code">Display.GetDivTag().appendChild(latencyOutput);</xsl:element>
        <xsl:element name="Code">}</xsl:element>
        <xsl:element name="Code">if (isErrorMarked)</xsl:element>
        <xsl:element name="Code">Display.RemoveDisplayItem(ErrorMark);</xsl:element>
        <xsl:element name="Code">isErrorMarked = false;</xsl:element>
        <xsl:element name="Code">Display.RemoveDisplayItem(currentStimulus);</xsl:element>
        <xsl:element name="Code">EventUtil.removeHandler(Display.GetDivTag(), "keypress", currentHandler);</xsl:element>
        <xsl:element name="Code">EventList[++EventCtr].Execute();</xsl:element>
        <xsl:element name="Code">} else if (Display.IsLeftResponse(keyCode)) {</xsl:element>
        <xsl:element name="Code">if (!isErrorMarked) {</xsl:element>
        <xsl:element name="Code">Display.AddDisplayItem(ErrorMark);</xsl:element>
        <xsl:element name="Code">isErrorMarked = true;</xsl:element>
        <xsl:element name="Code">}</xsl:element>
        <xsl:element name="Code">}</xsl:element>
      </xsl:variable>
      <xsl:element name="FunctionBody">
        <xsl:for-each select="$functionBodyElems/Code">
          <xsl:element name="Code">
            <xsl:attribute name="LineNum" select="position()"/>
            <xsl:value-of select="."/>
          </xsl:element>
        </xsl:for-each>
      </xsl:element>
    </xsl:element>

    <xsl:element name="Function">
      <xsl:attribute name="FunctionName" select="'BeginIAT'"/>
      <xsl:element name="Params"/>
      <xsl:variable name="functionBodyElems">
        <xsl:element name="Code">InitImages();</xsl:element>
        <xsl:element name="Code">GenerateEventList();</xsl:element>
        <xsl:element name="Code">EventCtr = 0;</xsl:element>
        <xsl:element name="Code">EventList[EventCtr].Execute();</xsl:element>
        <!--
        <xsl:element name="Code">while (Display.divTag.firstChild)</xsl:element>
        <xsl:element name="Code">Display.divTag.removeChild(Display.divTag.firstChild);</xsl:element>
        <xsl:element name="Code">ClickToStartElement = document.createElement("h4");</xsl:element>
        <xsl:element name="Code">ClickToStartText = document.createTextNode("Click Here to Begin");</xsl:element>
        <xsl:element name="Code">ClickToStartElement.appendChild(ClickToStartText);</xsl:element>
        <xsl:element name="Code">Display.divTag.appendChild(ClickToStartElement);</xsl:element>
        <xsl:call-template name="GenerateEventInit">
          <xsl:with-param name="EventListNode" select="//IATEventList"/>
          <xsl:with-param name="randomization" select="//RandomizationType"/>
        </xsl:call-template>
        <xsl:element name="Code">currentHandler = function() {</xsl:element>
        <xsl:element name="Code">Display.divTag.removeChild(ClickToStartElement);</xsl:element>
        <xsl:element name="Code">EventUtil.removeHandler(Display.divTag, "click", currentHandler);</xsl:element>
        <xsl:element name="Code">EventList[EventCtr].Execute();</xsl:element>
        <xsl:element name="Code">};</xsl:element>
        <xsl:element name="Code">EventUtil.addHandler(Display.divTag, "click", currentHandler);</xsl:element>
        <xsl:element name="Code">Display.divTag.tabIndex = -1;</xsl:element>
        <xsl:element name="Code">Display.divTag.focus();</xsl:element>
        <xsl:element name="Code">var bodyTag = document.getElementById("bodyID");</xsl:element>
        <xsl:element name="Code">EventUtil.addHandler(bodyTag, "click", function() {</xsl:element>
        <xsl:element name="Code">Display.divTag.tabIndx = -1;</xsl:element>
        <xsl:element name="Code">Display.divTag.focus();</xsl:element>
        <xsl:element name="Code">});</xsl:element>
        <xsl:element name="Code">var containerDiv = document.getElementById("IATContainerDiv");</xsl:element>
        <xsl:element name="Code">EventUtil.addHandler(containerDiv, "click", function() {</xsl:element>
        <xsl:element name="Code">Display.divTag.tabIndex = -1;</xsl:element>
        <xsl:element name="Code">Display.divTag.focus();</xsl:element>
        <xsl:element name="Code">});</xsl:element>    -->
      </xsl:variable>
      <xsl:element name="FunctionBody">
        <xsl:for-each select="$functionBodyElems/Code">
          <xsl:element name="Code">
            <xsl:attribute name="LineNum" select="position()"/>
            <xsl:value-of select="."/>
          </xsl:element>
        </xsl:for-each>
      </xsl:element>
    </xsl:element>

    <xsl:element name="Function">
      <xsl:attribute name="FunctionName" select="'InitImages'"/>
      <xsl:element name="Params"/>
      <xsl:variable name="functionBodyElems">
        <xsl:for-each select="//DisplayItemList/IATDisplayItem">
          <xsl:element name="Code">
            <xsl:value-of select="concat('DI', ID, ' = new IATDI(', ID, ', ', X, ', ', Y, ', ', Width, ', ', Height, ');')"/>
          </xsl:element>
        </xsl:for-each>
        <xsl:variable name="paramList"
                      select="concat(//IATLayout/InteriorWidth, ', ', //IATLayout/InteriorHeight, ', ', //LeftResponseASCIIKeyCodeUpper, ', ', //LeftResponseASCIIKeyCodeLower, ', ', //RightResponseASCIIKeyCodeUpper, ', ', //RightResponseASCIIKeyCodeLower)"/>
        <xsl:element name="Code">
          <xsl:value-of select="concat('Display = new IATDisplay(', $paramList, ');')"/>
        </xsl:element>
        <xsl:element name="Code">
          <xsl:value-of select="concat('ErrorMark = DI', //ErrorMarkID, ';')"/>
        </xsl:element>
        <xsl:element name="Code">
          <xsl:value-of select="concat('ErrorMark.imgTag.id = DI', //ErrorMarkID, '.imgTag.id;')"/>
        </xsl:element>
      </xsl:variable>
      <xsl:element name="FunctionBody">
        <xsl:for-each select="$functionBodyElems/Code">
          <xsl:element name="Code">
            <xsl:attribute name="LineNum" select="position()"/>
            <xsl:value-of select="."/>
          </xsl:element>
        </xsl:for-each>
      </xsl:element>
    </xsl:element>

    <xsl:element name="Function">
      <xsl:attribute name="FunctionName" select="'GenerateEventList'" />
      <xsl:element name="Params" />
      <xsl:variable name="functionBodyElems">
        <xsl:element name="Code">EventCtr = 0;</xsl:element>
        <xsl:call-template name="GenerateEventInit" />
      </xsl:variable>
      <xsl:element name="FunctionBody">
        <xsl:for-each select="$functionBodyElems/Code">
          <xsl:element name="Code">
            <xsl:attribute name="LineNum" select="position()" />
            <xsl:value-of select="." />
          </xsl:element>
        </xsl:for-each>
      </xsl:element>
    </xsl:element>
  </xsl:variable>

  <xsl:template match="ConfigFile">
    <xsl:element name="CodeFile">
      <xsl:element name="VarEntries">
        <xsl:copy-of select="$GlobalAbbreviations"/>
      </xsl:element>
      <xsl:element name="Classes">
        <xsl:copy-of select="$Classes/Class" />
      </xsl:element>
      <xsl:element name="Fumctions">
        <xsl:copy-of select="$Functions/Function union $processItemFunctions/Function" />
      </xsl:element>
    </xsl:element>
  </xsl:template>

  <xsl:template name="processCode">
    <xsl:param name="code"/>
    <xsl:param name="type"/>
    <xsl:param name="delim"/>
    <xsl:variable name="codeList">
      <xsl:analyze-string select="$code" regex="(([\)\{{\}};])|else)\s*?&#xA;">
        <xsl:non-matching-substring>
          <xsl:variable name="line" select="normalize-space(.)"/>
          <xsl:choose>
            <xsl:when test="matches($line, '^var\s+?([A-Za-z_][A-Za-z0-9_]*)(\s*=((\s+|[^;=/,&#34;\(]+?|&#34;[^&#34;\n\r]*?&#34;|\(([^;=,&#34;]*?,?(&#34;[^\n\r&#34;]*?&#34;)?)+\)|/[^/\n]+?/)+?)?)*')">
              <xsl:analyze-string select="replace($line, '(var\s+?)(.+)', '$2')"
                                  regex="([A-Za-z_][A-Za-z0-9_]*)(\s*(=((\s+|[^;=/,&#34;\(]+?|&#34;[^&#34;\n\r]*?&#34;|\(([^;=,&#34;]*?,?(&#34;[^\n\r&#34;]*?&#34;)?)+\)|/[^/\n]+?/)*)+?)?)">
                <xsl:matching-substring>
                  <xsl:element name="code">
                    <xsl:attribute name="type" select="'varName'"/>
                    <xsl:value-of select="regex-group(1)" disable-output-escaping="yes"/>
                  </xsl:element>
                  <xsl:if test="string-length(regex-group(2)) gt 0">
                    <xsl:element name="code">
                      <xsl:attribute name="type" select="'varAssign'"/>
                      <xsl:value-of select="regex-group(2)" disable-output-escaping="no"/>
                    </xsl:element>
                  </xsl:if>
                  <xsl:element name="code">
                    <xsl:attribute name="type" select="'subLineDelim'"/>
                    <xsl:value-of select="';'"/>
                  </xsl:element>
                </xsl:matching-substring>
              </xsl:analyze-string>
            </xsl:when>
            <xsl:when test="matches($line, '([^A-Za-z0-9_])(var\s+)([A-Za-z0-9][A-Za-z0-9_]+)')">
              <xsl:analyze-string select="$line" regex="([^A-Za-z0-9_])(var\s+)([A-Za-z0-9][A-Za-z0-9_]+)">
                <xsl:matching-substring>
                  <xsl:element name="code">
                    <xsl:attribute name="type" select="'code'"/>
                    <xsl:value-of select="regex-group(1)" disable-output-escaping="yes"/>
                  </xsl:element>
                  <xsl:element name="code">
                    <xsl:attribute name="type" select="'varName'"/>
                    <xsl:value-of select="regex-group(3)" disable-output-escaping="yes"/>
                  </xsl:element>
                </xsl:matching-substring>
                <xsl:non-matching-substring>
                  <xsl:element name="code">
                    <xsl:attribute name="type" select="'code'"/>
                    <xsl:value-of select="." disable-output-escaping="yes"/>
                  </xsl:element>
                </xsl:non-matching-substring>
              </xsl:analyze-string>
            </xsl:when>
            <xsl:otherwise>
              <xsl:element name="code">
                <xsl:attribute name="type" select="'code'"/>
                <xsl:value-of select="$line" disable-output-escaping="yes"/>
              </xsl:element>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:non-matching-substring>
        <xsl:matching-substring>
          <xsl:element name="code">
            <xsl:attribute name="type" select="'subLineDelim'"/>
            <xsl:value-of select="regex-group(1)" disable-output-escaping="yes"/>
          </xsl:element>
        </xsl:matching-substring>
      </xsl:analyze-string>
    </xsl:variable>
    <xsl:if test="(($type eq 'vars') or ($type eq 'both')) and (count($codeList/code[@type eq 'varName']) gt 0)">
      <xsl:value-of select="'var '"/>
      <xsl:for-each select="$codeList/code[((@type eq 'varName') and (every $var in preceding-sibling::code[@type eq 'varName'] satisfies normalize-space(.) ne normalize-space($var))) or (@type eq 'varAssign')]">
        <xsl:variable name="varName" select="."/>
        <xsl:if test="@type eq 'varName'">
          <xsl:choose>
            <xsl:when test="(position() eq last()) and (every $var in preceding-sibling::code[@type eq 'varName'] satisfies $var ne $varName)">
              <xsl:value-of select="." disable-output-escaping="no"/>
            </xsl:when>
            <xsl:when test="(count(following-sibling::code[@type eq 'varName']) gt 0) and (some $followingVar in following-sibling::code[@type eq 'varName'] satisfies ($followingVar ne $varName) and (every $precedingVar in preceding-sibling::code[@type eq 'varName'] satisfies $precedingVar ne $followingVar)) and (following-sibling::code[1]/@type ne 'varAssign')">
              <xsl:value-of select="concat(., ', ')" disable-output-escaping="yes"/>
            </xsl:when>
            <xsl:when test="following-sibling::code[1]/@type eq 'varAssign'">
              <xsl:variable name="assign" select="following-sibling::code[1]"/>
              <xsl:choose>
                <xsl:when test="some $var in preceding-sibling::code[@type eq 'varName'] satisfies (matches($assign, concat('[^A-Za-z0-9_]', normalize-space($var), '[^A-Za-z0-9_]?')) or matches($assign, concat('^', normalize-space($var), '[^A-Za-z0-9_]?')))">
                  <xsl:if test="every $elem in following-sibling::code[(@type eq 'varName') or (@type eq 'varAssign')] satisfies $elem/@type eq 'varAssign'">
                    <xsl:value-of select="." disable-output-escaping="yes"/>
                  </xsl:if>
                  <xsl:if test="some $elem in following-sibling::code[(@type eq 'varName') or (@type eq 'varAssign')] satisfies $elem/@type ne 'varAssign'">
                    <xsl:value-of select="concat(., ', ')"/>
                  </xsl:if>
                </xsl:when>
                <xsl:when test="(every $var in preceding-sibling::code[@type eq 'varName'] satisfies $var ne $varName) and (every $var in following-sibling::code[@type eq 'varName'] satisfies $var ne $varName)">
                  <xsl:if test="position() + 1 eq last()">
                    <xsl:value-of select="concat(., following-sibling::code[1])"/>
                  </xsl:if>
                  <xsl:if test="position() + 1 ne last()">
                    <xsl:value-of select="concat(., following-sibling::code[1], ', ')"/>
                  </xsl:if>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:choose>
                    <xsl:when test="(position() + 1 eq last()) or (every $followingVar in following-sibling::code[@type eq 'varName'] satisfies ($followingVar eq $varName) or (some $precedingVar in preceding-sibling::code[@type eq 'varName'] satisfies $followingVar eq $precedingVar))">
                      <xsl:value-of select="." disable-output-escaping="yes"/>
                    </xsl:when>
                    <xsl:when test="position() + 1 ne last()">
                      <xsl:value-of select="concat(., ', ')"/>
                    </xsl:when>
                  </xsl:choose>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:when>
          </xsl:choose>
        </xsl:if>
      </xsl:for-each>
      <xsl:value-of select="concat(';', $delim)"/>
    </xsl:if>
    <xsl:if test="($type eq 'code') or ($type eq 'both')">
      <xsl:for-each select="$codeList/code">
        <xsl:choose>
          <xsl:when test="@type eq 'code'">
            <xsl:value-of select="." disable-output-escaping="yes"/>
          </xsl:when>
          <xsl:when test="(@type eq 'varAssign') and (preceding-sibling::code[1]/@type eq 'varName')">
            <xsl:variable name="assign" select="normalize-space(.)"/>
            <xsl:variable name="thisVarName" select="preceding-sibling::code[@type eq 'varName'][1]"/>
            <xsl:choose>
              <xsl:when test="some $var in preceding-sibling::code[@type eq 'varName'][position() gt 1] satisfies matches($assign, concat('[^A-Za-z0-9_]?', normalize-space($var), '[^A-Za-z0-9_]?'))">
                <xsl:value-of select="concat(preceding-sibling::code[1], .)" disable-output-escaping="yes"/>
              </xsl:when>
              <xsl:when test="(some $var in preceding-sibling::code[@type eq 'varName'][position() gt 1] satisfies $var eq $thisVarName) or (some $var in following-sibling::code[@type eq 'varName'] satisfies $var eq $thisVarName)">
                <xsl:value-of select="concat(preceding-sibling::code[1], .)" disable-output-escaping="yes"/>
              </xsl:when>
            </xsl:choose>
          </xsl:when>
          <xsl:when test="(@type eq 'varName') and (position() gt 1)">
            <xsl:if test="preceding-sibling::code[1]/@type eq 'code'">
              <xsl:value-of select="." disable-output-escaping="yes"/>
            </xsl:if>
          </xsl:when>
          <xsl:when test="@type eq 'subLineDelim'">
            <xsl:if test="matches(., '^[^;]')">
              <xsl:value-of select="concat(., $delim)" disable-output-escaping="yes"/>
            </xsl:if>
            <xsl:if test="matches(., '^;')">
              <xsl:choose>
                <xsl:when test="preceding-sibling::code[1]/@type eq 'varAssign'">
                  <xsl:if test="preceding-sibling::code[2]/@type eq 'varName'">
                    <xsl:variable name="assign"
                                  select="normalize-space(preceding-sibling::code[@type eq 'varAssign'][1])"/>
                    <xsl:variable name="thisVarName" select="preceding-sibling::code[@type eq 'varName'][1]"/>
                    <xsl:choose>
                      <xsl:when test="some $var in preceding-sibling::code[@type eq 'varName'][position() gt 1] satisfies matches($assign, concat('[^A-Za-z0-9_]?', normalize-space($var), '[^A-Za-z0-9_]?'))">
                        <xsl:value-of select="concat(., $delim)" disable-output-escaping="yes"/>
                      </xsl:when>
                      <xsl:when test="(some $var in preceding-sibling::code[@type eq 'varName'][position() gt 1] satisfies $var eq $thisVarName) or (some $var in following-sibling::code[@type eq 'varName'] satisfies $var eq $thisVarName)">
                        <xsl:value-of select="concat(., $delim)" disable-output-escaping="yes"/>
                      </xsl:when>
                    </xsl:choose>
                  </xsl:if>
                </xsl:when>
                <xsl:when test="preceding-sibling::code[1]/@type eq 'varName'">
                  <xsl:variable name="varName" select="preceding-sibling::code[1]"/>
                  <xsl:if test="position() gt 2">
                    <xsl:if test="every $elem in preceding-sibling::code[@type eq 'varName'] satisfies $elem ne $varName">
                      <xsl:value-of select="concat(., $delim)" disable-output-escaping="yes"/>
                    </xsl:if>
                  </xsl:if>
                </xsl:when>
                <xsl:when test="preceding-sibling::code[1]/@type eq 'code'">
                  <xsl:value-of select="concat(., $delim)" disable-output-escaping="yes"/>
                </xsl:when>
              </xsl:choose>
            </xsl:if>
          </xsl:when>
        </xsl:choose>
      </xsl:for-each>
    </xsl:if>
  </xsl:template>

  <xsl:template name="MaskSpecifierArrayAppend">
    <xsl:param name="item"/>
    <xsl:param name="specifier" />
    <xsl:element name="Code">
      <xsl:value-of select="concat('KeyedDirInput = document.getElementById(DynamicKey', $specifier/ID, ').value;')"/>
    </xsl:element>
    <xsl:if test="$item/KeyedDir eq 'DynamicLeft'">
      <xsl:element name="Code">if (KeyedDirInput == "True") {</xsl:element>
      <xsl:element name="Code">MaskItemTrueArray.push(new Array());</xsl:element>
      <xsl:element name="Code">KeyedDir = "Left";</xsl:element>
      <xsl:element name="Code">} else {</xsl:element>
      <xsl:element name="Code">KeyedDir = "Right";</xsl:element>
      <xsl:element name="Code">MaskItemFalseArray.push(new Array());</xsl:element>
      <xsl:element name="Code">}</xsl:element>
    </xsl:if>
    <xsl:if test="$item/KeyedDir eq 'DynamicRight'">
      <xsl:element name="Code">if (KeyedDirInput == "True") {</xsl:element>
      <xsl:element name="Code">MaskItemTrueArray.push(new Array());</xsl:element>
      <xsl:element name="Code">KeyedDir = "Right";</xsl:element>
      <xsl:element name="Code">} else {</xsl:element>
      <xsl:element name="Code">KeyedDir = "Left";</xsl:element>
      <xsl:element name="Code">MaskItemFalseArray.push(new Array());</xsl:element>
      <xsl:element name="Code">}</xsl:element>
    </xsl:if>
    <xsl:variable name="params"
                  select="concat('itemCtr++, DI', $item/StimulusDisplayID, ', KeyedDir, ', $item/ItemNum, ', ', $item/OriginatingBlock)"/>
    <xsl:element name="Code">if (KeyedDirInput == "True")</xsl:element>
    <xsl:element name="Code">
      <xsl:value-of select="concat('MaskItemTrueArray[MaskItemTrueArray.length - 1].push(new Array(', $params, '));')" />
    </xsl:element>
    <xsl:element name="Code">else</xsl:element>
    <xsl:element name="Code">
      <xsl:value-of select="concat('MaskItemFalseArray[MaskItemTrueArray.length - 1].push(new Array(', $params, '));')"/>
    </xsl:element>
  </xsl:template>

  <xsl:template name="MaskSpecifierArrayAppendRange">
    <xsl:param name="items"/>
    <xsl:param name="specifier" />
    <xsl:element name="Code">
      <xsl:value-of select="concat('KeyedDirInput = document.getElementById(DynamicKey', $specifier/ID, ').value;')"/>
    </xsl:element>
    <xsl:for-each select="$items">
      <xsl:if test="KeyedDir eq 'DynamicLeft'">
        <xsl:element name="Code">if (KeyedDirInput == "True") {</xsl:element>
        <xsl:element name="Code">MaskItemTrueArray.push(new Array());</xsl:element>
        <xsl:element name="Code">KeyedDir = "Left";</xsl:element>
        <xsl:element name="Code">} else {</xsl:element>
        <xsl:element name="Code">KeyedDir = "Right";</xsl:element>
        <xsl:element name="Code">MaskItemFalseArray.push(new Array());</xsl:element>
        <xsl:element name="Code">}</xsl:element>
      </xsl:if>
      <xsl:if test="KeyedDir eq 'DynamicRight'">
        <xsl:element name="Code">if (KeyedDirInput == "True") {</xsl:element>
        <xsl:element name="Code">MaskItemTrueArray.push(new Array());</xsl:element>
        <xsl:element name="Code">KeyedDir = "Right";</xsl:element>
        <xsl:element name="Code">} else {</xsl:element>
        <xsl:element name="Code">KeyedDir = "Left";</xsl:element>
        <xsl:element name="Code">MaskItemFalseArray.push(new Array());</xsl:element>
        <xsl:element name="Code">}</xsl:element>
      </xsl:if>
      <xsl:variable name="params"
                    select="concat('itemCtr++, DI', StimulusDisplayID, ', KeyedDir, ', ItemNum, ', ', OriginatingBlock)"/>
      <xsl:element name="Code">if (KeyedDirInput == "True")</xsl:element>
      <xsl:element name="Code">
        <xsl:value-of select="concat('MaskItemTrueArray[MaskItemTrueArray.length - 1].push(new Array(', $params, '));')" />
      </xsl:element>
      <xsl:element name="Code">else</xsl:element>
      <xsl:element name="Code">
        <xsl:value-of select="concat('MaskItemFalseArray[MaskItemTrueArray.length - 1].push(new Array(', $params, '));')"/>
      </xsl:element>
    </xsl:for-each>
  </xsl:template>

  <xsl:template name="GenerateEventInit">
    <xsl:element name="Code">var iatBlock, instructionBlock, IATBlocks = new Array(), InstructionBlocks = new Array(), NumItemsAry = new Array(), piFunctions = new Array(), pifAry, ctr, ctr2, ctr3, randomNum, sourceAry = 1, iatItem, lesserAry, bAlternate, KeyedDir;</xsl:element>
    <xsl:element name="Code">bAlternate = (CookieUtil.get("Alternate") == "yes") ? true : false;</xsl:element>
    <xsl:for-each select="//IATEventList/IATEvent[@EventType eq 'BeginIATBlock']">
      <xsl:variable name="blockPosition" select="count(preceding-sibling::IATEvent[(@EventType eq 'BeginIATBlock') or (@EventType eq 'BeginInstructionBlock')])" />
      <xsl:variable name="numItems" select="NumItems" />
      <xsl:variable name="blockItems" select="following-sibling::IATEvent[position() le xs:integer($numItems)]" />
      <xsl:if test="//RandomizationType eq 'SetNumberOfPresentations'" >
        <xsl:element name="Code">
          <xsl:value-of select="concat('NumItemsAry.push(', NumPresentations, ');')" />
        </xsl:element>
      </xsl:if>
      <xsl:if test="//RandomizationType ne 'SetNumberOfPresentations'" >
        <xsl:element name="Code">
          <xsl:value-of select="concat('NumItemsAry.push(', NumItems, ');')" />
        </xsl:element>
      </xsl:if>
      <xsl:element name="Code">pifAry = new Array();</xsl:element>
      <xsl:element name="Code">piFunctions.push(pifAry);</xsl:element>
      <xsl:variable name="blockNum" select="BlockNum" />
      <xsl:for-each-group select="//DynamicSpecifiers/DynamicSpecifier" group-by="SurveyName">
        <xsl:variable name="surveyNum" select="position()" />
        <xsl:for-each-group select="current-group()" group-by="ItemNum">
          <xsl:if test="count(current-group()) eq 1">
            <xsl:if test="some $item in $blockItems satisfies $item/SpecifierID eq ID">
              <xsl:element name="Code">
                <xsl:value-of select="concat('pifAry.push(PDIF', $blockNum, '_', $surveyNum, '_', ItemNum, ');')" />
              </xsl:element>
            </xsl:if>
          </xsl:if>
          <xsl:if test="count(current-group()) gt 1">
            <xsl:if test="some $item in $blockItems satisfies (some $id in current-group()/ID satisfies $item/SpecifierID eq $id)" >
              <xsl:element name="Code">
                <xsl:value-of select="concat('pifAry.push(PDIF', $blockNum, '_', $surveyNum, '_', current-group()[1]/ItemNum, ');')" />
              </xsl:element>
            </xsl:if>
          </xsl:if>
        </xsl:for-each-group>
      </xsl:for-each-group>
      <xsl:if test="some $item in $blockItems satisfies $item/SpecifierID eq '-1'">
        <xsl:element name="Code">
          <xsl:value-of select="concat('pifAry.push(PIF', $blockNum, ');')" />
        </xsl:element>
      </xsl:if>
      <xsl:choose>
        <xsl:when test="(//Is7Block eq 'True') and (//RandomizationType eq 'SetNumberOfPresentations')">
          <xsl:element name="Code">
            <xsl:value-of select="concat('iatBlock = new IATBlock(', BlockNum, ', ', $blockPosition, ', ', NumPresentations, ', ', AlternatedWith, ');')"/>
          </xsl:element>
        </xsl:when>
        <xsl:when test="(//Is7Block eq 'False') and (//RandomizationType eq 'SetNumberOfPresentations')" >
          <xsl:element name="Code">
            <xsl:value-of select="concat('iatBlock = new IATBlock(', BlockNum, ', ', $blockPosition, ', ', NumPresentations, ', ', AlternatedWith, ');')" />
          </xsl:element>
        </xsl:when>
        <xsl:otherwise>
          <xsl:element name="Code">
            <xsl:value-of select="concat('iatBlock = push(new IATBlock(', BlockNum, ', ', $blockPosition, ', ',  NumItems, ', ', AlternatedWith, ');')" />
          </xsl:element>
        </xsl:otherwise>
      </xsl:choose>
      <xsl:element name="Code">
        <xsl:value-of select="concat('iatBlock.BeginBlockEvent = new IATBeginBlock(', lower-case(./PracticeBlock), ', DI', ./LeftResponseDisplayID, ', DI', ./RightResponseDisplayID, ', DI', ./InstructionsDisplayID, ');')"/>
      </xsl:element>
      <xsl:element name="Code">IATBlocks.push(iatBlock);</xsl:element>
    </xsl:for-each>
    <xsl:element name="Code">
      <xsl:value-of select="concat('for (ctr = 0; ctr &lt; ', count(//IATEventList/IATEvent[@EventType eq 'BeginIATBlock']), '; ctr++) {')" />
    </xsl:element>
    <xsl:choose>
      <xsl:when test="(//Is7Block eq 'True') and (//RandomizationType eq 'SetNumberOfPresentations')">
        <xsl:element name="Code">Items1 = new Array();</xsl:element>
        <xsl:element name="Code">Items2 = new Array();</xsl:element>
        <xsl:element name="Code">sourceAry = ((sourceAry == 2) || (ctr == 0)) ? 1 : 2;</xsl:element>
      </xsl:when>
      <xsl:otherwise>
        <xsl:element name="Code">Items = new Array();</xsl:element>
      </xsl:otherwise>
    </xsl:choose>
    <xsl:element name="Code">for (ctr2 = 0; ctr2 &lt; processIATItemFunctions[ctr].length; ctr2++)</xsl:element>
    <xsl:element name="Code">processIATItemFunctions[ctr][ctr2].call();</xsl:element>
    <xsl:element name="Code">if (Items1.length &lt; Items2.length)</xsl:element>
    <xsl:element name="Code">lesserAry = Items1;</xsl:element>
    <xsl:element name="Code">else</xsl:element>
    <xsl:element name="Code">lesserAry = Items2;</xsl:element>
    <xsl:element name="Code">for (ctr2 = 0; ctr2 &lt; NumItemsAry[ctr]; ctr2++) {</xsl:element>
    <xsl:choose>
      <xsl:when test="(//Is7Block eq 'True') and (//RandomizationType eq 'SetNumberOfPresentations')">
        <xsl:element name="Code">if (sourceAry == 1) {</xsl:element>
        <xsl:element name="Code">iatItem = Items1[Math.floor(Math.random() * Items1.length)];</xsl:element>
        <xsl:element name="Code">sourceAry = 2;</xsl:element>
        <xsl:element name="Code">} else {</xsl:element>
        <xsl:element name="Code">iatItem = Items2[Math.floor(Math.random() * Items2.length)];</xsl:element>
        <xsl:element name="Code">sourceAry = 1;</xsl:element>
        <xsl:element name="Code">}</xsl:element>
      </xsl:when>
      <xsl:otherwise>
        <xsl:element name="Code">iatItem = Items[Math.floor(Math.random() * Items.length)];</xsl:element>
      </xsl:otherwise>
    </xsl:choose>
    <xsl:element name="Code">ItemBlocks[ctr].AddItem(iatItem);</xsl:element>
    <xsl:element name="Code">}</xsl:element>
    <xsl:element name="Code">IATBlocks[ctr].EndBlockEvent = new IATEndBlock();</xsl:element>
    <xsl:element name="Code">}</xsl:element>
    <xsl:for-each select="//IATEventList/IATEvent[@EventType eq 'BeginIATBlock']">
      <xsl:variable name="blockPosition" select="count(preceding-sibling::IATEvent[(@EventType eq 'BeginInstructionBlock') or (@EventType eq 'BeginIATBlock')]) + 1" />
      <xsl:variable name="numScreens" select="xs:integer(NumInstructionScreens)" />
      <xsl:element name="Code">
        <xsl:value-of select="concat('instructionBlock = new IATInstructionBlock(', AlternatedWith, ', ', $blockPosition, ');')" />
      </xsl:element>
      <xsl:for-each select="following-sibling::IATEvent[position() le $numScreens]">
        <xsl:choose>
          <xsl:when test="@EventType eq 'TextInstructionScreen'">
            <xsl:element name="Code">
              <xsl:value-of select="concat('instructionBlock.AddScreen(&quot;Text&quot;, {', ContinueASCIIKeyCode, ', ', ContinueInstructionsDisplayID, ', ', InstructionsDisplayID, '});')" />
            </xsl:element>
          </xsl:when>
          <xsl:when test="@EventType eq 'KeyedInstructionScreen'">
            <xsl:element name="Code">
              <xsl:value-of select="concat('instructionBlock.AddScreen(&quot;Keyed&quot;, {', ContinueASCIIKeyCode, ', ', ContinueInstructionsDisplayID, ', ', InstructionsDisplayID, ', ', LeftResponseDisplayID, ', ', RightResponseDisplayID, '});')" />
            </xsl:element>
          </xsl:when>
          <xsl:when test="@EventType eq 'MockItemInstructionScreen'">
            <xsl:element name="Code">
              <xsl:value-of select="concat('instructionBlock.AddScreen(&quot;MockItem&quot;, {', ContinueASCIIKeyCode, ', ', ContinueInstructionsDisplayID, ', ', LeftResponseDisplayID, ', ', RightResponseDisplayID, ', ', StimulusDisplayID, ', ', InstructionsDisplayID, ', ', ErrorMarkIsDisplayed, ', ', OutlineLeftResponse, ', ', OutlineRightResponse, '});')" />
            </xsl:element>
          </xsl:when>
        </xsl:choose>
        <xsl:element name="Code">InstructionBlocks.push(instructionBlock);</xsl:element>
      </xsl:for-each>
    </xsl:for-each>
    <xsl:element name="Code">var itemBlockOrder, instructionBlockOrder;</xsl:element>
    <xsl:element name="Code">
      <xsl:variable name="alternationValues" select="string-join(//IATEventList/IATEvent[@EventType eq 'BeginIATBlock']/AlternatedWith, ', ')" />
      <xsl:value-of select="concat('itemBlockOrder = new Array(', $alternationValues, ');')" />
    </xsl:element>
    <xsl:element name="Code">
      <xsl:variable name="alternationValues" select="string-join(//IATEventList/IATEvent[@EventType eq 'BeginInstructionBlock']/AlternatedWith, ', ')" />
      <xsl:value-of select="concat('instructionBlockOrder = new Array(', $alternationValues, ');')"/>
    </xsl:element>
    <xsl:element name="Code">
      <xsl:value-of select="concat('for (ctr = 0; ctr &lt; ', xs:string(count(//IATEventList/IATEvent[(@EventType eq 'BeginIATBlock') or (@EventType eq 'BeginInstructionBlock')]) + 1), '; ctr++) {')" />
    </xsl:element>
    <xsl:element name="Code">var itemBlockCtr = 0, instructionBlockCtr = 0, ndx;</xsl:element>
    <xsl:element name="Code">if (ctr == ItemBlocks[itemBlockCtr].blockPosition) {</xsl:element>
    <xsl:element name="Code">if (bAlternate)</xsl:element>
    <xsl:element name="Code">ndx = (itemBlockOrder[itemBlockCtr++] == -1) ? itemBlockCtr: itemBlockOrder[itemBlockCtr] - 1;</xsl:element>
    <xsl:element name="Code">else</xsl:element>
    <xsl:element name="Code">ndx = itemBlockCtr++;</xsl:element>
    <xsl:element name="Code">EventList.push(IATBlocks[ndx].BeginBlockEvent);</xsl:element>
    <xsl:element name="Code">for (var ctr2 = 0; ctr2 &lt; IATBlocks[ndx].Items.length; ctr2++)</xsl:element>
    <xsl:element name="Code">EventList.push(IATBlocks[ndx].Items[ctr2]);</xsl:element>
    <xsl:element name="Code">EventList.push(IATBlocks[ndx].EndBlockEvent);</xsl:element>
    <xsl:element name="Code">} else {</xsl:element>
    <xsl:element name="Code">if (bAlternate)</xsl:element>
    <xsl:element name="Code">ndx = (instructionBlockOrder[instructionBlockCtr++] == -1) ? instructionBlockCtr : instructionBlockOrder[instructionBlockCtr] - 1;</xsl:element>
    <xsl:element name="Code">else</xsl:element>
    <xsl:element name="Code">ndx = instructionBlockCtr++;</xsl:element>
    <xsl:element name="Code">for (var ctr2 = 0; ctr2 &lt; InstructionBlocks[ndx].screens.length; ctr2++)</xsl:element>
    <xsl:element name="Code">EventList.push(InstructionBlocks[ndx].screens[ctr2]);</xsl:element>
    <xsl:element name="Code">}</xsl:element>
  </xsl:template>

  <xsl:template name="WriteVars">
    <xsl:param name="CodeLines" />
    <xsl:element name="Code">
      <xsl:value-of select="concat(string-join(for $i in 1 to count(CodeLines) return replace(CodeLines[$i], '(var(\s+)[A-Za-z_][A-Za-z0-9_]*)(.*)', '$1'), ', '), '&#x0A;')" />
    </xsl:element>
  </xsl:template>

  <xsl:template name="ProcessMaskSpecifiers">
    <xsl:element name="Code">var lesserLen, lesserAry, greaterAry, lesserLen, lesserLen2, lesserAry2, greaterAry2, randomNum, randomNum2, ctr2;</xsl:element>
    <xsl:element name="Code">if (MaskItemTrueArray.length &gt; MaskItemFalseArray.length) {</xsl:element>
    <xsl:element name="Code">lesserLen = MaskItemFalseArray.length;</xsl:element>
    <xsl:element name="Code">lesserAry = MaskItemFalseArray;</xsl:element>
    <xsl:element name="Code">geaterAry = MaskItemTrueArray;</xsl:element>
    <xsl:element name="Code">} else {</xsl:element>
    <xsl:element name="Code">lesserLen = MaskItemTrueArray.length;</xsl:element>
    <xsl:element name="Code">lesserAry = MaskItemTrueArray;</xsl:element>
    <xsl:element name="Code">greaterAry = MaskItemFalseArray;</xsl:element>
    <xsl:element name="Code">}</xsl:element>
    <xsl:element name="Code">if (lesserLen == 0)</xsl:element>
    <xsl:element name="Code">return;</xsl:element>
    <xsl:choose>
      <xsl:when test="(//Is7Block eq 'True') and (//RandomizationType eq 'SetNumberOfPresentations')" >
        <xsl:element name="Code">if (lesserAry[0][0][4] == 1)</xsl:element>
        <xsl:element name="Code">itemBlock = Items1;</xsl:element>
        <xsl:element name="Code">else</xsl:element>
        <xsl:element name="Code">itemBlock = Items2;</xsl:element>
      </xsl:when>
      <xsl:otherwise>
        <xsl:element name="Code">itemBlock = Items;</xsl:element>
      </xsl:otherwise>
    </xsl:choose>
    <xsl:element name="Code">for (ctr = 0; ctr &lt; lesserLen; ctr++) {</xsl:element>
    <xsl:element name="Code">randomNum = Math.floor(Math.random() * lesserLen);</xsl:element>
    <xsl:element name="Code">lesserAry2 = (lesserAry[ctr].length &gt; greaterAry[randomNum].length) ? greaterAry[randomNum] : lesserAry[ctr];</xsl:element>
    <xsl:element name="Code">greaterAry2 = (lesserAry[ctr].length &gt; greaterAry[randomNum].length) ? lesserAry[ctr] : greaterAry[randomNum];</xsl:element>
    <xsl:element name="Code">lesserLen2 = lesserAry2.length;</xsl:element>
    <xsl:element name="Code">for (ctr2 = 0; ctr2 &lt; lesserLen2; ctr2++) {</xsl:element>
    <xsl:element name="Code">randomNum2 = Math.floor(Math.random() * lesserLen2);</xsl:element>
    <xsl:element name="Code">itemBlock.push(new IATItem(lesserAry2[ctr2][0], lesserAry2[ctr2][1], lesserAry2[ctr2][3], lesserAry2[ctr2][2]));</xsl:element>
    <xsl:element name="Code">itemBlock.push(new IATItem(greterAry2[randomNum2][0], greaterAry2[randomNum2][1], greaterAry2[randomNum2][3], greaterAry2[randomNum2][2]));</xsl:element>
    <xsl:element name="Code">greaterAry2.splice(randomNum2, 1);</xsl:element>
    <xsl:element name="Code">}</xsl:element>
    <xsl:element name="Code">greaterAry.splice(randomNum, 1);</xsl:element>
    <xsl:element name="Code">}</xsl:element>
  </xsl:template>


  <xsl:template name="ProcessNoSpecItems" >
    <xsl:param name="items" />
    <xsl:for-each select="$items" >
      <xsl:variable name="params"
                    select="concat('EventCtr++, DI', ./StimulusDisplayID, ', ', ./ItemNum, ',  &quot;',  KeyedDir, '&quot;')"/>
      <xsl:choose>
        <xsl:when test="(//Is7Block eq 'True') and (//RandomizationType eq 'SetNumberOfPresentations')">
          <xsl:if test="OriginatingBlock eq '1'">
            <xsl:element name="Code">
              <xsl:value-of select="concat('Items1.push(new IATItem(', $params, '));')"/>
            </xsl:element>
          </xsl:if>
          <xsl:if test="OriginatingBlock eq '2'">
            <xsl:element name="Code">
              <xsl:value-of select="concat('Items2.push(new IATItem(', $params, '));')"/>
            </xsl:element>
          </xsl:if>
        </xsl:when>
        <xsl:otherwise>
          <xsl:element name="Code">
            <xsl:value-of select="concat('Items.push(new IATItem(', $params, '));')"/>
          </xsl:element>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:for-each>
  </xsl:template>

  <xsl:template name="ProcessTrueFalseSpecItems" >
    <xsl:param name="items" />
    <xsl:variable name="specifier" select="//DynamicSpecifier[every $i in $items satisfies $i/SpecifierID eq ID]" />
    <xsl:element name="Code">var randomNum, TrueFalseAry = new Array();</xsl:element>
    <xsl:element name="Code">
      <xsl:value-of select="concat('KeyedDirInput = document.getElementById(DynamicKey', $specifier/ID, ');')"/>
    </xsl:element>
    <xsl:for-each select="$items" >
      <xsl:if test="KeyedDir eq 'DynamicLeft'">
        <xsl:element name="Code">if (KeyedDirInput == "True")</xsl:element>
        <xsl:element name="Code">KeyedDir = "Left";</xsl:element>
        <xsl:element name="Code">else</xsl:element>
        <xsl:element name="Code">KeyedDir = "Right";</xsl:element>
      </xsl:if>
      <xsl:if test="KeyedDir eq 'DynamicRight'">
        <xsl:element name="Code">if (KeyedDirInput == "False")</xsl:element>
        <xsl:element name="Code">KeyedDir = "Left";</xsl:element>
        <xsl:element name="Code">else</xsl:element>
        <xsl:element name="Code">KeyedDir = "Right";</xsl:element>
      </xsl:if>
      <xsl:variable name="params" select="concat('EventCtr++, DI', StimulusDisplayID, ', ', ItemNum, ', KeyedDir, ', OriginatingBlock)" />
      <xsl:element name="Code">
        <xsl:value-of select="concat('TrueFalseAry.push(new Array(', $params, '));')" />
      </xsl:element>
    </xsl:for-each>
    <xsl:element name="Code">randomNum = Math.floor(Math.random() * TrueFalseAry.length);</xsl:element>
    <xsl:choose>
      <xsl:when test="(//Is7Block eq 'True') and (//RandomizationType eq 'SetNumberOfPresentations')" >
        <xsl:element name="Code">if (TrueFalseAry[randomNum][4] == 1)</xsl:element>
        <xsl:element name="Code">Items1.push(new IATItem(TrueFalseAry[randomNum][0], TrueFalseAry[randomNum][1], TrueFalseAry[randomNum][3], TrueFalseAry[randomNum][2]));</xsl:element>
        <xsl:element name="Code">if (TrueFalseAry[randomNum][4] == 2)</xsl:element>
        <xsl:element name="Code">Items2.push(new IATItem(TrueFalseAry[randomNum][0], TrueFalseAry[randomNum][1], TrueFalseAry[randomNum][3], TrueFalseAry[randomNum][2]));</xsl:element>
      </xsl:when>
      <xsl:otherwise>
        <xsl:element name="Code">Items.push(new IATItem(TrueFalseAry[randomNum][0], TrueFalseAry[randomNum][1], TrueFalseAry[randomNum][3], TrueFalseAry[randomNum][2]));</xsl:element>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="ProcessRangeSpecItems" >
    <xsl:param name="items" />
    <xsl:variable name="specifier" select="//DynamicSpecifier[every $i in $items satisfies $i/SpecifierID eq ID]" />
    <xsl:element name="Code">
      <xsl:value-of select="concat('KeyedDirInput = document.getElementById(DynamicKey', $specifier/ID, ');')"/>
    </xsl:element>
    <xsl:element name="Code">if (KeyedDirInput.value == "Exclude")</xsl:element>
    <xsl:element name="Code">return;</xsl:element>
    <xsl:element name="Code">var RangeItemAry = new Array();</xsl:element>
    <xsl:for-each select="$items">
      <xsl:if test="KeyedDir eq 'DynamicLeft'">
        <xsl:element name="Code">if (KeyedDirInput == "True")</xsl:element>
        <xsl:element name="Code">KeyedDir = "Left";</xsl:element>
        <xsl:element name="Code">else</xsl:element>
        <xsl:element name="Code">KeyedDir = "Right";</xsl:element>
      </xsl:if>
      <xsl:if test="KeyedDir eq 'DynamicRight'">
        <xsl:element name="Code">if (KeyedDirInput == "False")</xsl:element>
        <xsl:element name="Code">KeyedDir = "Right";</xsl:element>
        <xsl:element name="Code">else</xsl:element>
        <xsl:element name="Code">KeyedDir = "Left";</xsl:element>
      </xsl:if>
      <xsl:variable name="params">
        <xsl:value-of select="concat('itemCtr++, DI', StimulusDisplayID, ', ', ItemNum, ', KeyedDir, ', OriginatingBlock, ');')" />
      </xsl:variable>
      <xsl:element name="Code">
        <xsl:value-of select="concat('RangeItemAry.push(', $params, ');')"/>
      </xsl:element>
    </xsl:for-each>
    <xsl:element name="Code">var randomNum = Math.floor(Math.random() * RangeItemAry.length);</xsl:element>
    <xsl:choose>
      <xsl:when test="(//Is7Block eq 'True') and (//RandomizationType eq 'SetNumberOfPresentations')" >
        <xsl:element name="Code">if (RangeItemAry[randomNum][4] == 1)</xsl:element>
        <xsl:element name="Code">Items1.push(new IATItem(RangeItemAry[randomNum][0], RangeItemAry[randomNum][1], RangeItemAry[randomNum][3], RangeItemAry[randomNum][2]));</xsl:element>
        <xsl:element name="Code">if (RangeItemAry[randomNum][4] == 2)</xsl:element>
        <xsl:element name="Code">Items2.push(new IATItem(RangeItemAry[randomNum][0], RangeItemAry[randomNum][1], RangeItemAry[randomNum][3], RangeItemAry[randomNum][2]));</xsl:element>
      </xsl:when>
      <xsl:otherwise>
        <xsl:element name="Code">Items.push(new IATItem(RangeItemAry[randomNum][0], RangeItemAry[randomNum][1], RangeItemAry[randomNum][3], RangeItemAry[randomNum][2]));</xsl:element>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="ProcessMaskSpecItems" >
    <xsl:param name="items" />
    <xsl:element name="Code">var MaskItemTrueArray = new Array();</xsl:element>
    <xsl:element name="Code">var MaskItemFalseArray = new Array();</xsl:element>
    <xsl:for-each-group select="$items" group-by="SpecifierID" >
      <xsl:variable name="specificSpecifier" select="//DynamicSpecifier[every $i in current-group() satisfies $i/SpecifierID eq ID]"/>
      <xsl:if test="count(current-group()) eq 1">
        <xsl:call-template name="MaskSpecifierArrayAppend">
          <xsl:with-param name="item" select="current-group()"/>
          <xsl:with-param name="specifier" select="$specificSpecifier" />
        </xsl:call-template>
      </xsl:if>
      <xsl:if test="count(current-group()) gt 1">
        <xsl:call-template name="MaskSpecifierArrayAppendRange">
          <xsl:with-param name="items" select="current-group() "/>
          <xsl:with-param name="specifier" select="$specificSpecifier" />
        </xsl:call-template>
      </xsl:if>
    </xsl:for-each-group>
    <xsl:call-template name="ProcessMaskSpecifiers" />
  </xsl:template>

  <xsl:template name="ProcessSelectionSpecItems" >
    <xsl:param name="items" />
    <xsl:variable name="specifier" select="//DynamicSpecifier[every $i in $items satisfies $i/SpecifierID eq ID]" />
    <xsl:element name="Code">
      <xsl:value-of select="concat('var SelectedItem = document.getElementById(DynamicKey', $specifier/ID, ').value;')"/>
    </xsl:element>
    <xsl:element name="Code">var RandomItem = SelectedItem;</xsl:element>
    <xsl:element name="Code">var SelectionStimulusArray = new Array(), lesser, lesserLen, ndx1, ndx2, itemBlock;</xsl:element>
    <xsl:for-each-group select="$items" group-by="SpecifierArg" >
      <xsl:element name="Code">SelectionStimulusArray.push(new Array());</xsl:element>
      <xsl:if test="count(current-group()) eq 1" >
        <xsl:variable name="params"
                      select="concat(./SpecifierArg, ', DI', StimulusDisplayID, ', ', KeyedDir, ', ', ItemNum, ', ', ./OriginatingBlock)"/>
        <xsl:element name="Code">
          <xsl:value-of select="concat('SelectionStimulusArray[SelectionStimulusArray.length].push(new Array(', $params, '));')" />
        </xsl:element>
      </xsl:if>
      <xsl:if test="count(current-group()) gt 1" >
        <xsl:for-each select="current-group()" >
          <xsl:variable name="params"
                      select="concat(./SpecifierArg, ', DI', StimulusDisplayID, ', ', KeyedDir, ', ', ItemNum, ', ', ./OriginatingBlock)"/>
          <xsl:element name="Code">
            <xsl:value-of select="concat('SelectionStimulusArray[SelectionStimulusArray.length].push(new Array(', $params, '));')" />
          </xsl:element>
        </xsl:for-each>
      </xsl:if>
    </xsl:for-each-group>
    <xsl:element name="Code">
      <xsl:value-of select="concat('SelectedItem = parseInt(document.getElementById(DynamicKey', $specifier/ID, ').value, 10);')" />
    </xsl:element>
    <xsl:element name="Code">RandomItem = SelectedItem;</xsl:element>
    <xsl:element name="Code">while (RandomItem == SelectedItem)</xsl:element>
    <xsl:element name="Code">RandomItem = Math.floor(Math.random() * SelectionStimulusArray.length);</xsl:element>
    <xsl:element name="Code">if (SelectionStimulusArray[RandomItem].length &gt; SelectionStimlusArray[SelectedItem].length) {</xsl:element>
    <xsl:element name="Code">lesser = SelectedItem;</xsl:element>
    <xsl:element name="Code">lesserLen = SelectionStimulusArray[SelectedItem].length;</xsl:element>
    <xsl:element name="Code">} else if (SelectionStimulusArray[RandomItem].length &lt;= SelectionStimulusArray[SelectedItem].length) {</xsl:element>
    <xsl:element name="Code">lesser = RandomItem;</xsl:element>
    <xsl:element name="Code">lesserLen = SelectionStimulusArray[RandomItem].length;</xsl:element>
    <xsl:element name="Code">}</xsl:element>
    <xsl:element name="Code">for (ctr = 0; ctr &lt; lesser; ctr++) {</xsl:element>
    <xsl:element name="Code">if (lesser == SelectedItem) {</xsl:element>
    <xsl:element name="Code">ndx1 = ctr;</xsl:element>
    <xsl:element name="Code">ndx2 = Math.floor(Math.random() * SelectionStimulusArray[RandomItem].length);</xsl:element>
    <xsl:element name="Code">} else {</xsl:element>
    <xsl:element name="Code">ndx1 = Math.floor(Math.random() * SelectionStimulusArray[SelectedItem].length);</xsl:element>
    <xsl:element name="Code">ndx2 = ctr;</xsl:element>
    <xsl:element name="Code">}</xsl:element>
    <xsl:choose>
      <xsl:when test="(//Is7Block eq 'True') and (//RandomizationType eq 'SetNumberOfPresentations')">
        <xsl:element name="Code">if (SelectionStimulusArray[SelectedItem][ndx1][4] == 1)</xsl:element>
        <xsl:element name="Code">itemBlock = Items1;</xsl:element>
        <xsl:element name="Code">else</xsl:element>
        <xsl:element name="Code">itemBlock = Items2;</xsl:element>
      </xsl:when>
      <xsl:otherwise>
        <xsl:element name="Code">itemBlock = Items;</xsl:element>
      </xsl:otherwise>
    </xsl:choose>
    <xsl:element name="Code">if (SelectionStimulusArray[SelectedItem][ndx1][2] == "DynamicLeft")</xsl:element>
    <xsl:element name="Code">KeyedDir = "Left";</xsl:element>
    <xsl:element name="Code">else</xsl:element>
    <xsl:element name="Code">KeyedDir = "Right";</xsl:element>
    <xsl:element name="Code">itemBlock.push(new IATItem(EventCtr++, SelectionStimulusArray[SelectedItem][ndx1][1], SelectionStimulusArray[SelectedItem][ndx1][3], KeyedDir));</xsl:element>
    <xsl:element name="Code">if (SelectionStimulusArray[RandomItem][ndx2][2] == "DynamicLeft")</xsl:element>
    <xsl:element name="Code">KeyedDir = "Right";</xsl:element>
    <xsl:element name="Code">else</xsl:element>
    <xsl:element name="Code">KeyedDir = "Left";</xsl:element>
    <xsl:element name="Code">itemBlock.push(new IATItem(EventCtr++, SelectionStimulusArray[RandomItem][ndx2][1], SelectionStimulusArray[RandomItem][ndx2][3], KeyedDir));</xsl:element>
    <xsl:element name="Code">if (lesser == SelectedItem)</xsl:element>
    <xsl:element name="Code">SelectionStimulusArray[RandomItem].splice(ndx2, 1);</xsl:element>
    <xsl:element name="Code">else</xsl:element>
    <xsl:element name="Code">SelectionStimulusArray[SelectedItem].splice(ndx1, 1);</xsl:element>
    <xsl:element name="Code">}</xsl:element>
  </xsl:template>

  <xsl:template name="GenerateProcessItemFunctions">
    <xsl:for-each select="for $i in 1 to count(//IATEvent[@EventType eq 'BeginIATBlock']) return //IATEvent[@EventType eq 'BeginIATBlock'][$i]">
      <xsl:variable name="i" select="count(preceding-sibling::IATEvent[@EventType eq 'BeginIATBlock']) + 1" />
      <xsl:element name="ProcessItemsFunctions">
        <xsl:attribute name="BlockNum" select="$i" />
        <xsl:variable name="blockStart" select="//IATEvent[@EventType eq 'BeginIATBlock'][1 + count(preceding-sibling::IATEvent[@EventType eq 'BeginIATBlock']) eq $i]" />
        <xsl:variable name="items" select="$blockStart/following-sibling::IATEvent[@EventType eq 'IATItem'][position() le xs:integer($blockStart/NumItems)]" />
        <xsl:for-each-group select="//DynamicSpecifier[some $e in $items satisfies $e/SpecifierID eq ID]" group-by="SurveyName" >
          <xsl:variable name="surveyNum" select="position()" />
          <xsl:for-each-group select="current-group()" group-by="ItemNum" >
            <xsl:variable name="surveyItemNum" select="ItemNum" />
            <xsl:variable name="specType" select="if (count(current-group()) gt 1) then @SpecifierType else current-group()[last()]/@SpecifierType" />
            <xsl:element name="Function">
              <xsl:attribute name="FunctionName" select="concat('PDIF', $i, '_', $surveyNum, '_', $surveyItemNum)" />
              <xsl:element name="Params"/>
              <xsl:element name="FunctionBody">
                <xsl:choose>
                  <xsl:when test="$specType eq 'Mask'">
                    <xsl:call-template name="ProcessMaskSpecItems">
                      <xsl:with-param name="items" select="$items[some $spec in current-group() satisfies $spec/ID eq SpecifierID]" />
                    </xsl:call-template>
                  </xsl:when>

                  <xsl:when test="$specType eq 'Selection'">
                    <xsl:call-template name="ProcessSelectionSpecItems">
                      <xsl:with-param name="items" select="$items[some $spec in current-group() satisfies $spec/ID eq SpecifierID]" />
                    </xsl:call-template>
                  </xsl:when>

                  <xsl:when test="$specType eq 'TrueFalse'">
                    <xsl:call-template name="ProcessTrueFalseSpecItems" >
                      <xsl:with-param name="items" select="$items[some $spec in current-group() satisfies $spec/ID eq SpecifierID]" />
                    </xsl:call-template>
                  </xsl:when>

                  <xsl:when test="$specType eq 'Range'">
                    <xsl:call-template name="ProcessRangeSpecItems" >
                      <xsl:with-param name="items" select="$items[some $spec in current-group() satisfies $spec/ID eq SpecifierID]" />
                    </xsl:call-template>
                  </xsl:when>
                </xsl:choose>
              </xsl:element>
            </xsl:element>
          </xsl:for-each-group>
        </xsl:for-each-group>
        <xsl:if test="count($items[SpecifierID eq xs:string(-1)]) gt 0">
          <xsl:element name="Function">
            <xsl:attribute name="FunctionName" select="concat('PIF', $i)" />
            <xsl:element name="Params"/>
            <xsl:element name="FunctionBody">
              <xsl:call-template name="ProcessNoSpecItems">
                <xsl:with-param name="items" select="$items[SpecifierID eq xs:string(-1)]" />
              </xsl:call-template>
            </xsl:element>
          </xsl:element>
        </xsl:if>
      </xsl:element>
    </xsl:for-each>
  </xsl:template>
</xsl:stylesheet>