import React, { useState } from 'react';
import ReactDOM from 'react-dom'
import { createRoot } from 'react-dom/client';
import { Footer } from './page/footer/template';

const person = { name:"John", surname:"Doe", age:27 }

const TwoParagraphs = () =>
  <>
    {JSON.stringify(person)}
    <p>Paragraph 1</p>
    <p>Paragraph 2</p>
  </>

const AList = () =>
  <ul>
    <li key="1">Item 1</li>
    <li key="2">Item 2</li>
    <li key="3">Item 3</li>
    <li key="4">Item 4</li>
    <li key="5">Item 5</li>
  </ul>

const Header = () =>
  <>
    <h1>I am the header</h1>
  </>

const MainContent = () =>
  <>
    <div>I am the main content of the page</div>
    <TwoParagraphs />
    <AList />
  </>  

const Page = () =>
  <>
    <Header />
    <MainContent />
    <Footer />
  </>  


export const main = () => {
  const rootElement = document.querySelector('#root')
  if (!rootElement) { alert("Cannot find root element!"); return }
  const root = createRoot(rootElement)
  root.render(
    <div className='page'>
      <Page />
    </div>
  )
}
