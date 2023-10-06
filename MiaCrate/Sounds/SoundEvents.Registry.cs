using MiaCrate.Core;

namespace MiaCrate.Sounds;

public static partial class SoundEvents
{
    public static SoundEvent AllayAmbientWithItem { get; } = Register("entity.allay.ambient_with_item");
    public static SoundEvent AllayAmbientWithoutItem { get; } = Register("entity.allay.ambient_without_item");
    public static SoundEvent AllayDeath { get; } = Register("entity.allay.death");
    public static SoundEvent AllayHurt { get; } = Register("entity.allay.hurt");
    public static SoundEvent AllayItemGiven { get; } = Register("entity.allay.item_given");
    public static SoundEvent AllayItemTaken { get; } = Register("entity.allay.item_taken");
    public static SoundEvent AllayThrow { get; } = Register("entity.allay.item_thrown");

    public static IReferenceHolder<SoundEvent> AmbientCave { get; } = RegisterForHolder("ambient.cave");
    public static IReferenceHolder<SoundEvent> AmbientBasaltDeltasAdditions { get; } = RegisterForHolder("ambient.basalt_deltas.additions");
    public static IReferenceHolder<SoundEvent> AmbientBasaltDeltasLoop { get; } = RegisterForHolder("ambient.basalt_deltas.loop");
    public static IReferenceHolder<SoundEvent> AmbientBasaltDeltasMood { get; } = RegisterForHolder("ambient.basalt_deltas.mood");
    public static IReferenceHolder<SoundEvent> AmbientCrimsonForestAdditions { get; } = RegisterForHolder("ambient.crimson_forest.additions");
    public static IReferenceHolder<SoundEvent> AmbientCrimsonForestLoop { get; } = RegisterForHolder("ambient.crimson_forest.loop");
    public static IReferenceHolder<SoundEvent> AmbientCrimsonForestMood { get; } = RegisterForHolder("ambient.crimson_forest.mood");
    public static IReferenceHolder<SoundEvent> AmbientNetherWastesAdditions { get; } = RegisterForHolder("ambient.nether_wastes.additions");
    public static IReferenceHolder<SoundEvent> AmbientNetherWastesLoop { get; } = RegisterForHolder("ambient.nether_wastes.loop");
    public static IReferenceHolder<SoundEvent> AmbientNetherWastesMood { get; } = RegisterForHolder("ambient.nether_wastes.mood");
    public static IReferenceHolder<SoundEvent> AmbientSoulSandValleyAdditions { get; } = RegisterForHolder("ambient.soul_sand_valley.additions");
    public static IReferenceHolder<SoundEvent> AmbientSoulSandValleyLoop { get; } = RegisterForHolder("ambient.soul_sand_valley.loop");
    public static IReferenceHolder<SoundEvent> AmbientSoulSandValleyMood { get; } = RegisterForHolder("ambient.soul_sand_valley.mood");
    public static IReferenceHolder<SoundEvent> AmbientWarpedForestAdditions { get; } = RegisterForHolder("ambient.warped_forest.additions");
    public static IReferenceHolder<SoundEvent> AmbientWarpedForestLoop { get; } = RegisterForHolder("ambient.warped_forest.loop");
    public static IReferenceHolder<SoundEvent> AmbientWarpedForestMood { get; } = RegisterForHolder("ambient.warped_forest.mood");

    public static SoundEvent AmbientUnderwaterEnter { get; } = Register("ambient.underwater.enter");
    public static SoundEvent AmbientUnderwaterExit { get; } = Register("ambient.underwater.exit");
    public static SoundEvent AmbientUnderwaterLoop { get; } = Register("ambient.underwater.loop");
    public static SoundEvent AmbientUnderwaterLoopAdditions { get; } = Register("ambient.underwater.loop.additions");
    public static SoundEvent AmbientUnderwaterLoopAdditionsRare { get; } = Register("ambient.underwater.loop.additions.rare");
    public static SoundEvent AmbientUnderwaterLoopAdditionsUltraRare { get; } = Register("ambient.underwater.loop.additions.ultra_rare");

    public static SoundEvent AmethystBlockBreak { get; } = Register("block.amethyst_block.break");
    public static SoundEvent AmethystBlockChime { get; } = Register("block.amethyst_block.chime");
    public static SoundEvent AmethystBlockFall { get; } = Register("block.amethyst_block.fall");
    public static SoundEvent AmethystBlockHit { get; } = Register("block.amethyst_block.hit");
    public static SoundEvent AmethystBlockPlace { get; } = Register("block.amethyst_block.place");
    public static SoundEvent AmethystBlockResonate { get; } = Register("block.amethyst_block.resonate");
    public static SoundEvent AmethystBlockStep { get; } = Register("block.amethyst_block.step");
    
    public static SoundEvent AmethystClusterBreak { get; } = Register("block.amethyst_cluster.break");
    public static SoundEvent AmethystClusterFall { get; } = Register("block.amethyst_cluster.fall");
    public static SoundEvent AmethystClusterHit { get; } = Register("block.amethyst_cluster.hit");
    public static SoundEvent AmethystClusterPlace { get; } = Register("block.amethyst_cluster.place");
    public static SoundEvent AmethystClusterStep { get; } = Register("block.amethyst_cluster.step");
    
    public static SoundEvent AncientDebrisBreak { get; } = Register("block.ancient_debris.break");
    public static SoundEvent AncientDebrisFall { get; } = Register("block.ancient_debris.fall");
    public static SoundEvent AncientDebrisHit { get; } = Register("block.ancient_debris.hit");
    public static SoundEvent AncientDebrisPlace { get; } = Register("block.ancient_debris.place");
    public static SoundEvent AncientDebrisStep { get; } = Register("block.ancient_debris.step");
    
    public static SoundEvent AnvilBreak { get; } = Register("block.anvil.break");
    public static SoundEvent AnvilDestroy { get; } = Register("block.anvil.destroy");
    public static SoundEvent AnvilFall { get; } = Register("block.anvil.fall");
    public static SoundEvent AnvilHit { get; } = Register("block.anvil.hit");
    public static SoundEvent AnvilLand { get; } = Register("block.anvil.land");
    public static SoundEvent AnvilPlace { get; } = Register("block.anvil.place");
    public static SoundEvent AnvilStep { get; } = Register("block.anvil.step");
    public static SoundEvent AnvilUse { get; } = Register("block.anvil.use");

    public static SoundEvent ArmorEquipChain { get; } = Register("item.armor.equip_chain");
    public static SoundEvent ArmorEquipDiamond { get; } = Register("item.armor.equip_diamond");
    public static SoundEvent ArmorEquipElytra { get; } = Register("item.armor.equip_elytra");
    public static SoundEvent ArmorEquipGeneric { get; } = Register("item.armor.equip_generic");
    public static SoundEvent ArmorEquipGold { get; } = Register("item.armor.equip_gold");
    public static SoundEvent ArmorEquipIron { get; } = Register("item.armor.equip_iron");
    public static SoundEvent ArmorEquipLeather { get; } = Register("item.armor.equip_leather");
    public static SoundEvent ArmorEquipNetherite { get; } = Register("item.armor.equip_netherite");
    public static SoundEvent ArmorEquipTurtle { get; } = Register("item.armor.equip_turtle");
    
    public static SoundEvent ArmorStandBreak { get; } = Register("entity.armor_stand.break");
    public static SoundEvent ArmorStandFall { get; } = Register("entity.armor_stand.fall");
    public static SoundEvent ArmorStandHit { get; } = Register("entity.armor_stand.hit");
    public static SoundEvent ArmorStandPlace { get; } = Register("entity.armor_stand.place");

    public static SoundEvent ArrowHit { get; } = Register("entity.arrow.hit");
    public static SoundEvent ArrowHitPlayer { get; } = Register("entity.arrow.hit_player");
    public static SoundEvent ArrowShoot { get; } = Register("entity.arrow.shoot");

    public static SoundEvent AxeStrip { get; } = Register("item.axe.strip");
    public static SoundEvent AxeScrape { get; } = Register("item.axe.scrape");
    public static SoundEvent AxeWaxOff { get; } = Register("item.axe.wax_off");

    public static SoundEvent AxolotlAttack { get; } = Register("entity.axolotl.attack");
    public static SoundEvent AxolotlDeath { get; } = Register("entity.axolotl.death");
    public static SoundEvent AxolotlHurt { get; } = Register("entity.axolotl.hurt");
    public static SoundEvent AxolotlIdleAir { get; } = Register("entity.axolotl.idle_air");
    public static SoundEvent AxolotlIdleWater { get; } = Register("entity.axolotl.idle_water");
    public static SoundEvent AxolotlSplash { get; } = Register("entity.axolotl.splash");
    public static SoundEvent AxolotlSwim { get; } = Register("entity.axolotl.swim");
    
    public static SoundEvent AzaleaBreak { get; } = Register("block.azalea.break");
    public static SoundEvent AzaleaFall { get; } = Register("block.azalea.fall");
    public static SoundEvent AzaleaHit { get; } = Register("block.azalea.hit");
    public static SoundEvent AzaleaPlace { get; } = Register("block.azalea.place");
    public static SoundEvent AzaleaStep { get; } = Register("block.azalea.step");
    
    public static SoundEvent AzaleaLeavesBreak { get; } = Register("block.azalea_leaves.break");
    public static SoundEvent AzaleaLeavesFall { get; } = Register("block.azalea_leaves.fall");
    public static SoundEvent AzaleaLeavesHit { get; } = Register("block.azalea_leaves.hit");
    public static SoundEvent AzaleaLeavesPlace { get; } = Register("block.azalea_leaves.place");
    public static SoundEvent AzaleaLeavesStep { get; } = Register("block.azalea_leaves.step");
    
    public static SoundEvent BambooBreak { get; } = Register("block.bamboo.break");
    public static SoundEvent BambooFall { get; } = Register("block.bamboo.fall");
    public static SoundEvent BambooHit { get; } = Register("block.bamboo.hit");
    public static SoundEvent BambooPlace { get; } = Register("block.bamboo.place");
    public static SoundEvent BambooStep { get; } = Register("block.bamboo.step");
    
    public static SoundEvent BambooSaplingBreak { get; } = Register("block.bamboo_sapling.break");
    public static SoundEvent BambooSaplingHit { get; } = Register("block.bamboo_sapling.hit");
    public static SoundEvent BambooSaplingPlace { get; } = Register("block.bamboo_sapling.place");
    
    public static SoundEvent BambooWoodBreak { get; } = Register("block.bamboo_wood.break");
    public static SoundEvent BambooWoodFall { get; } = Register("block.bamboo_wood.fall");
    public static SoundEvent BambooWoodHit { get; } = Register("block.bamboo_wood.hit");
    public static SoundEvent BambooWoodPlace { get; } = Register("block.bamboo_wood.place");
    public static SoundEvent BambooWoodStep { get; } = Register("block.bamboo_wood.step");

    public static SoundEvent BambooWoodDoorClose { get; } = Register("block.bamboo_wood_door.close");
    public static SoundEvent BambooWoodDoorOpen { get; } = Register("block.bamboo_wood_door.open");
    
    public static SoundEvent BambooWoodTrapdoorClose { get; } = Register("block.bamboo_wood_trapdoor.close");
    public static SoundEvent BambooWoodTrapdoorOpen { get; } = Register("block.bamboo_wood_trapdoor.open");

    public static SoundEvent BambooWoodButtonClickOff { get; } = Register("block.bamboo_wood_button.click_off");
    public static SoundEvent BambooWoodButtonClickOn { get; } = Register("block.bamboo_wood_button.click_on");
    
    public static SoundEvent BambooWoodPressurePlateClickOff { get; } = Register("block.bamboo_wood_pressure_plate.click_off");
    public static SoundEvent BambooWoodPressurePlateClickOn { get; } = Register("block.bamboo_wood_pressure_plate.click_on");
    
    public static SoundEvent BambooWoodFenceGateClose { get; } = Register("block.bamboo_wood_fence_gate.close");
    public static SoundEvent BambooWoodFenceGateOpen { get; } = Register("block.bamboo_wood_fence_gate.open");
    
    public static SoundEvent BarrelClose { get; } = Register("block.barrel.close");
    public static SoundEvent BarrelOpen { get; } = Register("block.barrel.open");
    
    public static SoundEvent BasaltBreak { get; } = Register("block.basalt.break");
    public static SoundEvent BasaltStep { get; } = Register("block.basalt.step");
    public static SoundEvent BasaltPlace { get; } = Register("block.basalt.place");
    public static SoundEvent BasaltHit { get; } = Register("block.basalt.hit");
    public static SoundEvent BasaltFall { get; } = Register("block.basalt.fall");

    public static SoundEvent BatAmbient { get; } = Register("entity.bat.ambient");
    public static SoundEvent BatDeath { get; } = Register("entity.bat.death");
    public static SoundEvent BatHurt { get; } = Register("entity.bat.hurt");
    public static SoundEvent BatLoop { get; } = Register("entity.bat.loop");
    public static SoundEvent BatTakeOff { get; } = Register("entity.bat.takeoff");

    public static SoundEvent BeaconActivate { get; } = Register("block.beacon.activate");
    public static SoundEvent BeaconAmbient { get; } = Register("block.beacon.ambient");
    public static SoundEvent BeaconDeactivate { get; } = Register("block.beacon.deactivate");
    public static SoundEvent BeaconPowerSelect { get; } = Register("block.beacon.power_select");

    public static SoundEvent BeeDeath { get; } = Register("entity.bee.death");
    public static SoundEvent BeeHurt { get; } = Register("entity.bee.hurt");
    public static SoundEvent BeeLoopAggressive { get; } = Register("entity.bee.loop_aggressive");
    public static SoundEvent BeeLoop { get; } = Register("entity.bee.loop");
    public static SoundEvent BeeSting { get; } = Register("entity.bee.sting");
    public static SoundEvent BeePollinate { get; } = Register("entity.bee.pollinate");

    public static SoundEvent BeehiveDrip { get; } = Register("block.beehive.drip");
    public static SoundEvent BeehiveEnter { get; } = Register("block.beehive.enter");
    public static SoundEvent BeehiveExit { get; } = Register("block.beehive.exit");
    public static SoundEvent BeehiveShear { get; } = Register("block.beehive.shear");
    public static SoundEvent BeehiveWork { get; } = Register("block.beehive.work");

    public static SoundEvent BellBlock { get; } = Register("block.bell.use");
    public static SoundEvent BellResonate { get; } = Register("block.bell.resonate");
    
    public static SoundEvent BigDripleafBreak { get; } = Register("block.big_dripleaf.break");
    public static SoundEvent BigDripleafFall { get; } = Register("block.big_dripleaf.fall");
    public static SoundEvent BigDripleafHit { get; } = Register("block.big_dripleaf.hit");
    public static SoundEvent BigDripleafPlace { get; } = Register("block.big_dripleaf.place");
    public static SoundEvent BigDripleafStep { get; } = Register("block.big_dripleaf.step");
    
    public static SoundEvent BlazeAmbient { get; } = Register("entity.blaze.ambient");
    public static SoundEvent BlazeBurn { get; } = Register("entity.blaze.burn");
    public static SoundEvent BlazeDeath { get; } = Register("entity.blaze.death");
    public static SoundEvent BlazeHurt { get; } = Register("entity.blaze.hurt");
    public static SoundEvent BlazeShoot { get; } = Register("entity.blaze.shoot");

    public static List<IReferenceHolder<SoundEvent>> GoatHornSoundVariants { get; } = RegisterGoatHornSoundVariants();

    public static SoundEvent ItemBreak { get; } = Register("entity.item.break");
    public static SoundEvent ItemPickup { get; } = Register("entity.item.pickup");
    
    public static SoundEvent PlayerAttackCrit { get; } = Register("entity.player.attack.crit");
    public static SoundEvent PlayerAttackKnockback { get; } = Register("entity.player.attack.knockback");
    public static SoundEvent PlayerAttackNoDamage { get; } = Register("entity.player.attack.nodamage");
    public static SoundEvent PlayerAttackStrong { get; } = Register("entity.player.attack.strong");
    public static SoundEvent PlayerAttackSweep { get; } = Register("entity.player.attack.sweep");
    public static SoundEvent PlayerAttackWeak { get; } = Register("entity.player.attack.weak");
    public static SoundEvent PlayerBigFall { get; } = Register("entity.player.big_fall");
    public static SoundEvent PlayerBurp { get; } = Register("entity.player.burp");
    public static SoundEvent PlayerBreath { get; } = Register("entity.player.breath");
    public static SoundEvent PlayerDeath { get; } = Register("entity.player.death");
    public static SoundEvent PlayerHurt { get; } = Register("entity.player.hurt");
    public static SoundEvent PlayerHurtDrown { get; } = Register("entity.player.hurt_drown");
    public static SoundEvent PlayerHurtFreeze { get; } = Register("entity.player.hurt_freeze");
    public static SoundEvent PlayerHurtOnFire { get; } = Register("entity.player.hurt_on_fire");
    public static SoundEvent PlayerHurtSweetBerryBush { get; } = Register("entity.player.hurt_sweet_berry_bush");
    public static SoundEvent PlayerLevelUp { get; } = Register("entity.player.levelup");
    public static SoundEvent PlayerSmallFall { get; } = Register("entity.player.small_fall");
    public static SoundEvent PlayerSplash { get; } = Register("entity.player.splash");
    public static SoundEvent PlayerSplashHighSpeed { get; } = Register("entity.player.splash.high_speed");
    public static SoundEvent PlayerSwim { get; } = Register("entity.player.swim");

    public static SoundEvent StrayAmbient { get; } = Register("entity.stray.ambient");
    public static SoundEvent StrayDeath { get; } = Register("entity.stray.death");
    public static SoundEvent StrayHurt { get; } = Register("entity.stray.hurt");
    public static SoundEvent StrayStep { get; } = Register("entity.stray.step");
    
    public static SoundEvent ThornsHit { get; } = Register("enchant.thorns.hit");
    
    public static IReferenceHolder<SoundEvent> UiButtonClick { get; } = RegisterForHolder("ui.button.click");
    public static SoundEvent UiLoomSelectPattern { get; } = Register("ui.loom.select_pattern");
    public static SoundEvent UiLoomTakeResult { get; } = Register("ui.loom.take_result");
    public static SoundEvent UiCartographyTableTakeResult { get; } = Register("ui.cartography_table.take_result");
    public static SoundEvent UiStonecutterTakeResult { get; } = Register("ui.stonecutter.take_result");
    public static SoundEvent UiStonecutterSelectRecipe { get; } = Register("ui.stonecutter.select_recipe");
    public static SoundEvent UiToastChallengeComplete { get; } = Register("ui.toast.challenge_complete");
    public static SoundEvent UiToastIn { get; } = Register("ui.toast.in");
    public static SoundEvent UiToastOut { get; } = Register("ui.toast.out");

    public static SoundEvent ZombieVillagerAmbient { get; } = Register("entity.zombie_villager.ambient");
    public static SoundEvent ZombieVillagerConverted { get; } = Register("entity.zombie_villager.converted");
    public static SoundEvent ZombieVillagerCure { get; } = Register("entity.zombie_villager.cure");
    public static SoundEvent ZombieVillagerDeath { get; } = Register("entity.zombie_villager.death");
    public static SoundEvent ZombieVillagerHurt { get; } = Register("entity.zombie_villager.hurt");
    public static SoundEvent ZombieVillagerStep { get; } = Register("entity.zombie_villager.step");
}