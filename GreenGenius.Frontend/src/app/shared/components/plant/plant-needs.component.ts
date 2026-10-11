import { Component, input } from '@angular/core';
import { NgIcon } from '@ng-icons/core';
import { TranslatePipe } from '@ngx-translate/core';
import { PlantDto, RootDepthLevel, SunlightLevel, WaterNeedLevel } from '../../../api/api-client.generated';

export const SUNLIGHT_LABELS: Record<SunlightLevel, string> = {
  [SunlightLevel.Low]: 'plants.needs.sunlight.low',
  [SunlightLevel.Medium]: 'plants.needs.sunlight.medium',
  [SunlightLevel.High]: 'plants.needs.sunlight.high',
};

export const SUNLIGHT_ICONS: Record<SunlightLevel, string> = {
  [SunlightLevel.Low]: 'lucideCloud',
  [SunlightLevel.Medium]: 'lucideCloudSun',
  [SunlightLevel.High]: 'lucideSun',
};

export const WATER_NEED_LABELS: Record<WaterNeedLevel, string> = {
  [WaterNeedLevel.Low]: 'plants.needs.water-need.low',
  [WaterNeedLevel.Medium]: 'plants.needs.water-need.medium',
  [WaterNeedLevel.High]: 'plants.needs.water-need.high',
};

export const ROOT_DEPTH_LABELS: Record<RootDepthLevel, string> = {
  [RootDepthLevel.Shallow]: 'plants.needs.root-depth.shallow',
  [RootDepthLevel.Medium]: 'plants.needs.root-depth.medium',
  [RootDepthLevel.Deep]: 'plants.needs.root-depth.deep',
};

export const ROOT_DEPTH_ICONS: Record<RootDepthLevel, string> = {
  [RootDepthLevel.Shallow]: 'lucideChevronDown',
  [RootDepthLevel.Medium]: 'lucideChevronsDown',
  [RootDepthLevel.Deep]: 'lucideArrowDown',
};

export const WATER_NEED_DROPLETS: Record<WaterNeedLevel, number> = {
  [WaterNeedLevel.Low]: 1,
  [WaterNeedLevel.Medium]: 2,
  [WaterNeedLevel.High]: 3,
};

@Component({
  imports: [NgIcon, TranslatePipe],
  selector: 'app-plant-needs',
  templateUrl: './plant-needs.html',
})
export class PlantNeedsComponent {
  readonly plant = input.required<PlantDto>();

  readonly SUNLIGHT_LABELS = SUNLIGHT_LABELS;
  readonly SUNLIGHT_ICONS = SUNLIGHT_ICONS;
  readonly WATER_NEED_LABELS = WATER_NEED_LABELS;
  readonly ROOT_DEPTH_LABELS = ROOT_DEPTH_LABELS;
  readonly ROOT_DEPTH_ICONS = ROOT_DEPTH_ICONS;
  readonly WATER_NEED_DROPLETS = WATER_NEED_DROPLETS;

  dropletIndices(count: number): number[] {
    return Array.from({ length: count }, (_, k) => k);
  }
}
