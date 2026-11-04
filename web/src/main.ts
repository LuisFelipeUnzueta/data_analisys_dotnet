// O CSS vem antes do App: o tema dos gráficos lê os tokens CSS ao carregar.
import './app.css';
import { mount } from 'svelte';
import App from './App.svelte';

mount(App, { target: document.getElementById('app')! });
