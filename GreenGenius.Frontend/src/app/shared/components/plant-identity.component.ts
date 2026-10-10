import { Component, computed, effect, inject, input, OnInit, signal } from '@angular/core';
import { NgIcon } from '@ng-icons/core';
import { firstValueFrom } from 'rxjs';
import { TranslatePipe } from '@ngx-translate/core';
import { Client, PlantDto, PlantFamily, PlantLifeCycle, RootDepthLevel, SunlightLevel, WaterNeedLevel } from '../../api/api-client.generated';
import { IdentitiesComponent, Row } from './identities.component';
import { NeedsComponent } from './needs.component';
import { DimensionsComponent } from './dimensions.component';

@Component({
  imports: [TranslatePipe, IdentitiesComponent, NeedsComponent, DimensionsComponent],
  selector: 'app-plant-identity',
  templateUrl: './plant-identity.html',
})
export class PlantIdentityComponent implements OnInit {
  private readonly client = inject(Client);

  readonly id = input<string>();

  readonly plant = signal<PlantDto | null | undefined>(undefined);

  readonly loading = signal(false);

  private readonly loadEffect = effect(() => {
    const id = this.id();
    this.loadPlant();
  });

  ngOnInit(): void {
    this.loadPlant();
  }

  private dropIndices(count: number | undefined): number[] {
    return Array.from({ length: count ?? 1 }, (_, i) => i);
  }

  private loadPlant(): void {
    const id = this.id();
    if (!id) return;
    this.loading.set(true);
    firstValueFrom(this.client.getPlant(id)).then((value) => {
      this.plant.set(value);
      this.loading.set(false);
    }).catch(() => {
      this.plant.set(null);
      this.loading.set(false);
    });
  }

  readonly identityRows = computed<Array<{ labelKey: string; valueKey?: string; value?: string; badge?: Badge; sectionKey?: string }>>(() => {
    const plant = this.plant();
    if (plant === null || plant === undefined) return [];

    const rows: Array<{ labelKey: string; valueKey?: string; value?: string; badge?: Badge; sectionKey?: string }> = [];

    if (plant.family) {
      rows.push({ labelKey: 'plants.family.label', valueKey: FAMILY_LABELS[plant.family], sectionKey: 'plants.section.identity' });
    }
    if (plant.lifeCycle) {
      rows.push({ labelKey: 'plants.life-cycle.label', valueKey: LIFE_CYCLE_LABELS[plant.lifeCycle], sectionKey: 'plants.section.identity' });
    }
    const days = plant.daysToMaturity;
    rows.push({
      labelKey: 'plants.days-to-maturity',
      valueKey: days === undefined || days === null ? NOT_SET_KEY : 'plants.unit.days',
      value: days === undefined || days === null ? undefined : String(days),
      sectionKey: 'plants.section.identity',
    });
    if (plant.needs?.sunlight) {
      rows.push({
        labelKey: 'plants.needs.sunlight.label',
        badge: { kind: 'sunlight', iconKey: SUNLIGHT_ICONS[plant.needs.sunlight], textKey: SUNLIGHT_LABELS[plant.needs.sunlight], title: 'plants.needs.sunlight.label' },
        sectionKey: 'plants.section.needs',
      });
    }
    if (plant.needs?.waterNeed) {
      rows.push({
        labelKey: 'plants.needs.water-need.label',
        badge: { kind: 'water-need', iconKey: WATER_NEED_ICONS[plant.needs.waterNeed], textKey: WATER_NEED_LABELS[plant.needs.waterNeed], title: 'plants.needs.water-need.label', count: WATER_NEED_LEVELS[plant.needs.waterNeed] },
        sectionKey: 'plants.section.needs',
      });
    }
    if (plant.needs?.rootDepth) {
      rows.push({
        labelKey: 'plants.needs.root-depth.label',
        badge: { kind: 'root-depth', iconKey: ROOT_DEPTH_ICONS[plant.needs.rootDepth], textKey: ROOT_DEPTH_LABELS[plant.needs.rootDepth], title: 'plants.needs.root-depth.label' },
        sectionKey: 'plants.section.needs',
      });
    }
    const soilPhMin = plant.needs?.soilPhMin;
    const soilPhMax = plant.needs?.soilPhMax;
    if (soilPhMin !== undefined && soilPhMin !== null && soilPhMax !== undefined && soilPhMax !== null) {
      rows.push({
        labelKey: 'plants.needs.soil-ph.badge',
        badge: {
          kind: 'soil-ph',
          iconKey: 'lucideFlaskConical',
          textKey: 'plants.needs.soil-ph.badge',
          title: 'plants.needs.soil-ph',
          params: { min: String(soilPhMin), max: String(soilPhMax) },
        },
        sectionKey: 'plants.section.needs',
      });
    }
    if (plant.needs?.spacingRowCm !== undefined && plant.needs?.spacingRowCm !== null) {
      rows.push({ labelKey: 'plants.needs.spacing-row', valueKey: 'plants.unit.cm', value: String(plant.needs.spacingRowCm), sectionKey: 'plants.section.dimensions' });
    }
    if (plant.needs?.spacingPlantCm !== undefined && plant.needs?.spacingPlantCm !== null) {
      rows.push({ labelKey: 'plants.needs.spacing-plant', valueKey: 'plants.unit.cm', value: String(plant.needs.spacingPlantCm), sectionKey: 'plants.section.dimensions' });
    }
    if (plant.needs?.heightCm !== undefined && plant.needs?.heightCm !== null) {
      rows.push({ labelKey: 'plants.needs.height', valueKey: 'plants.unit.cm', value: String(plant.needs.heightCm), sectionKey: 'plants.section.dimensions' });
    }
    if (plant.needs?.spreadCm !== undefined && plant.needs?.spreadCm !== null) {
      rows.push({ labelKey: 'plants.needs.spread', valueKey: 'plants.unit.cm', value: String(plant.needs.spreadCm), sectionKey: 'plants.section.dimensions' });
    }
    if (plant.traits?.nitrogenFixer) {
      rows.push({ labelKey: 'plants.traits.nitrogen-fixer', badge: { kind: 'trait', iconKey: 'lucideCheck', textKey: 'plants.traits.nitrogen-fixer', title: 'plants.traits.nitrogen-fixer' }, sectionKey: 'plants.section.strengths' });
    }
    if (plant.traits?.dynamicAccumulator) {
      rows.push({ labelKey: 'plants.traits.dynamic-accumulator', badge: { kind: 'trait', iconKey: 'lucideCheck', textKey: 'plants.traits.dynamic-accumulator', title: 'plants.traits.dynamic-accumulator' }, sectionKey: 'plants.section.strengths' });
    }
    if (plant.traits?.pollinatorFriendly) {
      rows.push({ labelKey: 'plants.traits.pollinator-friendly', badge: { kind: 'trait', iconKey: 'lucideCheck', textKey: 'plants.traits.pollinator-friendly', title: 'plants.traits.pollinator-friendly' }, sectionKey: 'plants.section.strengths' });
    }
    if (plant.traits?.droughtTolerant) {
      rows.push({ labelKey: 'plants.traits.drought-tolerant', badge: { kind: 'trait', iconKey: 'lucideCheck', textKey: 'plants.traits.drought-tolerant', title: 'plants.traits.drought-tolerant' }, sectionKey: 'plants.section.strengths' });
    }

    return rows;
  });

  readonly identitySection = computed<Row[]>(() =>
    this.identityRows().filter((row) => row.sectionKey === 'plants.section.identity'),
  );

  readonly needsSection = computed<Row[]>(() =>
    this.identityRows().filter((row) => row.sectionKey === 'plants.section.needs'),
  );

  readonly dimensionsSection = computed<Row[]>(() =>
    this.identityRows().filter((row) => row.sectionKey === 'plants.section.dimensions'),
  );

  readonly strengthsSection = computed<Row[]>(() =>
    this.identityRows().filter((row) => row.sectionKey === 'plants.section.strengths'),
  );
}

const FAMILY_LABELS: Record<PlantFamily, string> = {
  [PlantFamily.Solanaceae]: 'plants.family.solanaceae',
  [PlantFamily.Brassicaceae]: 'plants.family.brassicaceae',
  [PlantFamily.Fabaceae]: 'plants.family.fabaceae',
  [PlantFamily.Apiaceae]: 'plants.family.apiaceae',
  [PlantFamily.Cucurbitaceae]: 'plants.family.cucurbitaceae',
  [PlantFamily.Amaryllidaceae]: 'plants.family.amaryllidaceae',
  [PlantFamily.Asteraceae]: 'plants.family.asteraceae',
  [PlantFamily.Amaranthaceae]: 'plants.family.amaranthaceae',
  [PlantFamily.Poaceae]: 'plants.family.poaceae',
  [PlantFamily.Lamiaceae]: 'plants.family.lamiaceae',
  [PlantFamily.Rosaceae]: 'plants.family.rosaceae',
  [PlantFamily.Polygonaceae]: 'plants.family.polygonaceae',
  [PlantFamily.Other]: 'plants.family.other',
};

const LIFE_CYCLE_LABELS: Record<PlantLifeCycle, string> = {
  [PlantLifeCycle.Annual]: 'plants.life-cycle.annual',
  [PlantLifeCycle.Biennial]: 'plants.life-cycle.biennial',
  [PlantLifeCycle.Perennial]: 'plants.life-cycle.perennial',
};

export interface Badge {
  kind: 'sunlight' | 'water-need' | 'root-depth' | 'soil-ph' | 'trait';
  iconKey: string;
  textKey: string;
  title: string;
  count?: number;
  params?: Record<string, string | number>;
}

const SUNLIGHT_LABELS: Record<SunlightLevel, string> = {
  [SunlightLevel.Low]: 'plants.needs.sunlight.low',
  [SunlightLevel.Medium]: 'plants.needs.sunlight.medium',
  [SunlightLevel.High]: 'plants.needs.sunlight.high',
};

const SUNLIGHT_ICONS: Record<SunlightLevel, string> = {
  [SunlightLevel.Low]: 'lucideCloud',
  [SunlightLevel.Medium]: 'lucideCloudSun',
  [SunlightLevel.High]: 'lucideSun',
};

const WATER_NEED_LABELS: Record<WaterNeedLevel, string> = {
  [WaterNeedLevel.Low]: 'plants.needs.water-need.low',
  [WaterNeedLevel.Medium]: 'plants.needs.water-need.medium',
  [WaterNeedLevel.High]: 'plants.needs.water-need.high',
};

const WATER_NEED_ICONS: Record<WaterNeedLevel, string> = {
  [WaterNeedLevel.Low]: 'lucideDroplet',
  [WaterNeedLevel.Medium]: 'lucideDroplet',
  [WaterNeedLevel.High]: 'lucideDroplet',
};

const WATER_NEED_LEVELS: Record<WaterNeedLevel, number> = {
  [WaterNeedLevel.Low]: 1,
  [WaterNeedLevel.Medium]: 2,
  [WaterNeedLevel.High]: 3,
};

const ROOT_DEPTH_LABELS: Record<RootDepthLevel, string> = {
  [RootDepthLevel.Shallow]: 'plants.needs.root-depth.shallow',
  [RootDepthLevel.Medium]: 'plants.needs.root-depth.medium',
  [RootDepthLevel.Deep]: 'plants.needs.root-depth.deep',
};

const ROOT_DEPTH_ICONS: Record<RootDepthLevel, string> = {
  [RootDepthLevel.Shallow]: 'lucideChevronDown',
  [RootDepthLevel.Medium]: 'lucideChevronsDown',
  [RootDepthLevel.Deep]: 'lucideArrowDown',
};

const NOT_SET_KEY = 'common.not-set';
