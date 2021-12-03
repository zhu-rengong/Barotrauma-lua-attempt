-- luacheck: ignore 111

--[[--
Barotrauma.AfflictionPrefab
]]
-- @code AfflictionPrefab
-- @pragma nostrip
local AfflictionPrefab = {}

--- Dispose
-- @realm shared
function Dispose() end

--- LoadAll
-- @realm shared
-- @tparam Enumerable files
function AfflictionPrefab.LoadAll(files) end

--- LoadFromFile
-- @realm shared
-- @tparam ContentFile file
function AfflictionPrefab.LoadFromFile(file) end

--- RemoveByFile
-- @realm shared
-- @tparam string filePath
function AfflictionPrefab.RemoveByFile(filePath) end

--- ToString
-- @realm shared
-- @treturn string
function ToString() end

--- Instantiate
-- @realm shared
-- @tparam number strength
-- @tparam Character source
-- @treturn Affliction
function Instantiate(strength, source) end

--- GetActiveEffect
-- @realm shared
-- @tparam number currentStrength
-- @treturn Effect
function GetActiveEffect(currentStrength) end

--- GetTreatmentSuitability
-- @realm shared
-- @tparam Item item
-- @treturn number
function GetTreatmentSuitability(item) end

--- GetType
-- @realm shared
-- @treturn Type
function GetType() end

--- Equals
-- @realm shared
-- @tparam Object obj
-- @treturn bool
function Equals(obj) end

--- GetHashCode
-- @realm shared
-- @treturn number
function GetHashCode() end

---
-- AfflictionPrefab.List, Field of type Enumerable
-- @realm shared
-- @Enumerable AfflictionPrefab.List

---
-- FilePath, Field of type string
-- @realm shared
-- @string FilePath

---
-- UIntIdentifier, Field of type number
-- @realm shared
-- @number UIntIdentifier

---
-- Identifier, Field of type string
-- @realm shared
-- @string Identifier

---
-- OriginalName, Field of type string
-- @realm shared
-- @string OriginalName

---
-- ContentPackage, Field of type ContentPackage
-- @realm shared
-- @ContentPackage ContentPackage

---
-- Effects, Field of type Enumerable
-- @realm shared
-- @Enumerable Effects

---
-- PeriodicEffects, Field of type IList`1
-- @realm shared
-- @IList`1 PeriodicEffects

---
-- TreatmentSuitability, Field of type Enumerable
-- @realm shared
-- @Enumerable TreatmentSuitability

---
-- AfflictionPrefab.ListArray, Field of type AfflictionPrefab[]
-- @realm shared
-- @AfflictionPrefab[] AfflictionPrefab.ListArray

---
-- AfflictionType, Field of type string
-- @realm shared
-- @string AfflictionType

---
-- LimbSpecific, Field of type bool
-- @realm shared
-- @bool LimbSpecific

---
-- IndicatorLimb, Field of type LimbType
-- @realm shared
-- @LimbType IndicatorLimb

---
-- Name, Field of type string
-- @realm shared
-- @string Name

---
-- Description, Field of type string
-- @realm shared
-- @string Description

---
-- TranslationOverride, Field of type string
-- @realm shared
-- @string TranslationOverride

---
-- IsBuff, Field of type bool
-- @realm shared
-- @bool IsBuff

---
-- CauseOfDeathDescription, Field of type string
-- @realm shared
-- @string CauseOfDeathDescription

---
-- SelfCauseOfDeathDescription, Field of type string
-- @realm shared
-- @string SelfCauseOfDeathDescription

---
-- ActivationThreshold, Field of type number
-- @realm shared
-- @number ActivationThreshold

---
-- ShowIconThreshold, Field of type number
-- @realm shared
-- @number ShowIconThreshold

---
-- ShowIconToOthersThreshold, Field of type number
-- @realm shared
-- @number ShowIconToOthersThreshold

---
-- MaxStrength, Field of type number
-- @realm shared
-- @number MaxStrength

---
-- GrainBurst, Field of type number
-- @realm shared
-- @number GrainBurst

---
-- ShowInHealthScannerThreshold, Field of type number
-- @realm shared
-- @number ShowInHealthScannerThreshold

---
-- TreatmentThreshold, Field of type number
-- @realm shared
-- @number TreatmentThreshold

---
-- KarmaChangeOnApplied, Field of type number
-- @realm shared
-- @number KarmaChangeOnApplied

---
-- BurnOverlayAlpha, Field of type number
-- @realm shared
-- @number BurnOverlayAlpha

---
-- DamageOverlayAlpha, Field of type number
-- @realm shared
-- @number DamageOverlayAlpha

---
-- AchievementOnRemoved, Field of type string
-- @realm shared
-- @string AchievementOnRemoved

---
-- Icon, Field of type Sprite
-- @realm shared
-- @Sprite Icon

---
-- IconColors, Field of type Color[]
-- @realm shared
-- @Color[] IconColors

---
-- AfflictionOverlay, Field of type Sprite
-- @realm shared
-- @Sprite AfflictionOverlay

---
-- AfflictionOverlayAlphaIsLinear, Field of type bool
-- @realm shared
-- @bool AfflictionOverlayAlphaIsLinear

---
-- AfflictionPrefab.InternalDamage, Field of type AfflictionPrefab
-- @realm shared
-- @AfflictionPrefab AfflictionPrefab.InternalDamage

---
-- AfflictionPrefab.ImpactDamage, Field of type AfflictionPrefab
-- @realm shared
-- @AfflictionPrefab AfflictionPrefab.ImpactDamage

---
-- AfflictionPrefab.Bleeding, Field of type AfflictionPrefab
-- @realm shared
-- @AfflictionPrefab AfflictionPrefab.Bleeding

---
-- AfflictionPrefab.Burn, Field of type AfflictionPrefab
-- @realm shared
-- @AfflictionPrefab AfflictionPrefab.Burn

---
-- AfflictionPrefab.OxygenLow, Field of type AfflictionPrefab
-- @realm shared
-- @AfflictionPrefab AfflictionPrefab.OxygenLow

---
-- AfflictionPrefab.Bloodloss, Field of type AfflictionPrefab
-- @realm shared
-- @AfflictionPrefab AfflictionPrefab.Bloodloss

---
-- AfflictionPrefab.Pressure, Field of type AfflictionPrefab
-- @realm shared
-- @AfflictionPrefab AfflictionPrefab.Pressure

---
-- AfflictionPrefab.Stun, Field of type AfflictionPrefab
-- @realm shared
-- @AfflictionPrefab AfflictionPrefab.Stun

---
-- AfflictionPrefab.RadiationSickness, Field of type AfflictionPrefab
-- @realm shared
-- @AfflictionPrefab AfflictionPrefab.RadiationSickness

---
-- AfflictionPrefab.Prefabs, Field of type PrefabCollection`1
-- @realm shared
-- @PrefabCollection`1 AfflictionPrefab.Prefabs

