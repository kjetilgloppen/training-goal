<script setup lang="ts">
import { onMounted, onBeforeUnmount, ref, watch } from 'vue'
import {
  Chart,
  LineController,
  LineElement,
  PointElement,
  LinearScale,
  Tooltip,
  Legend,
  Filler,
} from 'chart.js'
import type { Goal } from '../types'
import { buildChartSeries } from '../pace'

Chart.register(LineController, LineElement, PointElement, LinearScale, Tooltip, Legend, Filler)

const props = defineProps<{ goal: Goal }>()
const canvas = ref<HTMLCanvasElement | null>(null)
let chart: Chart | null = null

function accent(): string {
  return getComputedStyle(document.documentElement).getPropertyValue('--accent').trim() || '#7c3aed'
}

function render() {
  if (!canvas.value) return
  const s = buildChartSeries(props.goal)

  chart?.destroy()
  chart = new Chart(canvas.value, {
    type: 'line',
    data: {
      datasets: [
        {
          label: 'On pace',
          data: s.expected,
          borderColor: '#9ca3af',
          borderDash: [6, 6],
          pointRadius: 0,
          tension: 0,
        },
        {
          label: 'Actual',
          data: s.actual,
          borderColor: accent(),
          backgroundColor: 'rgba(124, 58, 237, 0.08)',
          pointRadius: 0,
          fill: true,
          tension: 0,
        },
      ],
    },
    options: {
      responsive: true,
      maintainAspectRatio: false,
      interaction: { mode: 'nearest', intersect: false },
      scales: {
        x: {
          type: 'linear',
          min: 0,
          max: s.totalDays,
          ticks: {
            maxTicksLimit: 6,
            callback: (v) => s.labelForDay(Number(v)),
          },
          grid: { display: false },
        },
        y: {
          min: 0,
          suggestedMax: s.target,
          ticks: { precision: 0 },
        },
      },
      plugins: {
        legend: { position: 'bottom' },
        tooltip: {
          callbacks: {
            title: (items) => s.labelForDay(Number(items[0].parsed.x)),
            label: (item) => `${item.dataset.label}: ${Math.round(Number(item.parsed.y))}`,
          },
        },
      },
    },
  })
}

onMounted(render)
watch(() => props.goal, render, { deep: true })
onBeforeUnmount(() => chart?.destroy())
</script>

<template>
  <div style="height: 260px">
    <canvas ref="canvas"></canvas>
  </div>
</template>
